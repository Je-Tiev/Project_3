using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.Participant;
using System.Security.Claims;
using System.Collections.Concurrent;
using Paperless_Meeting.Services.Poll;

namespace Paperless_Meeting.Hubs
{
    [Authorize]
    public class MeetingHub : Hub
    {
        private readonly IMeetingService _meetingService;
        private readonly IParticipantService _participantService;
        private readonly IPollService _pollService;
        private readonly ILogger<MeetingHub> _logger;
        // <RoomName, <ConnectionId, UserId>>
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _roomConnections = new();
        // <RoomName, ScreenShareConnectionId>
        private static readonly ConcurrentDictionary<string, string> _roomScreenShares = new();

        public MeetingHub(IMeetingService meetingService,
                          IParticipantService participantService,
                          IPollService pollService,
                          ILogger<MeetingHub> logger)
        {
            _meetingService = meetingService;
            _participantService = participantService;
            _pollService = pollService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? Context.User?.FindFirst("nameidentifier")?.Value;

            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            _logger.LogWarning("[Hub Warning] User ID claim not found or invalid. ConnectionId={ConnectionId}", Context.ConnectionId);
            throw new HubException("User ID claim not found or invalid.");
        }
        // Tham gia phòng họp
        public async Task JoinRoom(int meetingId)
        {
            _logger.LogInformation("[HUB INFO] JoinRoom called: MeetingId={MeetingId}, Connection={ConnectionId}", meetingId, Context.ConnectionId);
            try
            {
                var userId = GetUserId();
                _logger.LogDebug("[DEBUG] UserId from token: {UserId}", userId);
                // 1. Lấy thông tin cuộc họp
                var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
                if (meeting == null)
                {
                    _logger.LogWarning("[DEBUG] Meeting {MeetingId} not found", meetingId);
                    throw new HubException("Không tìm thấy cuộc họp.");
                }

                _logger.LogDebug("[DEBUG] Meeting RoomId={RoomId}, Start={Start}, End={End}", meeting.RoomId, meeting.StartTime, meeting.EndTime);
                // 2. Kiểm tra quyền tham gia
                if (string.IsNullOrEmpty(meeting.RoomId))
                {
                    _logger.LogWarning("[DEBUG] Meeting RoomId is null or empty");
                    throw new HubException("Cuộc họp chưa được cấu hình RoomId.");
                }
                // 3. Kiểm tra thời gian cuộc họp
                var now = DateTime.UtcNow;
                if (now < meeting.StartTime || now > meeting.EndTime)
                {
                    _logger.LogWarning("[DEBUG] Current UTC time {Now} is outside meeting time", now);
                    throw new HubException("Hiện không phải thời gian diễn ra cuộc họp.");
                }
                // 4. Kiểm tra người dùng có trong danh sách tham gia không
                var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
                if (participants == null || !participants.Any(p => p.UserId == userId))
                {
                    var participantIds = participants != null ? string.Join(", ", participants.Select(p => p.UserId)) : "null";
                    _logger.LogWarning("[DEBUG] Participants: {ParticipantIds}", participantIds);
                    throw new HubException("Bạn không có quyền tham gia cuộc họp này.");
                }
                // 5. Thêm người dùng vào group SignalR
                var roomName = meeting.RoomId;
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                // 6. Quản lý kết nối 
                // roomDict: <ConnectionId, UserId>
                var roomDict = _roomConnections.GetOrAdd(roomName, _ => new ConcurrentDictionary<string, int>());
                roomDict[Context.ConnectionId] = userId;

                var userFullName = Context.User?.FindFirst(ClaimTypes.Name)?.Value
                                   ?? Context.User?.FindFirst(ClaimTypes.GivenName)?.Value
                                   ?? $"User_{userId}";
                // 7. Thông báo cho các thành viên khác trong phòng
                await Clients.OthersInGroup(roomName).SendAsync("UserJoined", new
                {
                    userId,
                    connectionId = Context.ConnectionId,
                    fullName = userFullName
                });
                // 8. Gửi danh sách thành viên hiện có trong phòng cho người mới tham gia
                var existingConnections = roomDict
                    .Where(kvp => kvp.Key != Context.ConnectionId)
                    .Select(kvp => new
                    {
                        connectionId = kvp.Key,
                        userId = kvp.Value,
                        fullName = $"User_{kvp.Value}"
                    })
                    .ToList();

                await Clients.Caller.SendAsync("ExistingParticipants", existingConnections);
                // 8a. Kiểm tra có ai đang chia sẻ màn hình
                if (_roomScreenShares.TryGetValue(roomName, out string? sharerConnectionId))
                {
                    // Nếu phòng đang có người share, báo ngay cho người mới vào biết
                    await Clients.Caller.SendAsync("ScreenShareStarted", new
                    {
                        connectionId = sharerConnectionId, 
                        isResync = true, // Cờ để frontend biết đây là đồng bộ lại trạng thái cũ
                        timestamp = DateTime.Now
                    });
                }
                // 8b. Lấy các poll đang hoạt động trong cuộc họp
                var activePolls = await _pollService.GetActivePollsByMeetingIdAsync(meetingId, userId);
                if (activePolls != null && activePolls.Any())
                {
                    await Clients.Caller.SendAsync("SyncCurrentPolls", activePolls);
                }
                // 9. Gửi xác nhận đã tham gia phòng
                await Clients.Caller.SendAsync("JoinedRoom", new
                {
                    roomName,
                    meetingId,
                    message = "Tham gia thành công!"
                });

                _logger.LogInformation("[HUB INFO] User {UserId} joined room {RoomName} successfully", userId, roomName);
            }
            catch (HubException ex)
            {
                _logger.LogWarning("[HUB BUSINESS ERROR] MeetingId={MeetingId}, Message={Message}", meetingId, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HUB FATAL ERROR] MeetingId={MeetingId}", meetingId);
                throw new HubException("Lỗi không mong muốn khi tham gia cuộc họp. Kiểm tra log server.");
            }
        }

        public async Task SendSignal(string targetConnectionId, object signal)
        {
            var senderId = Context.ConnectionId;

            // 1. Kiểm tra target có tồn tại và khác sender không
            if (string.IsNullOrEmpty(targetConnectionId) || targetConnectionId == senderId)
            {
                _logger.LogWarning("[Signal Warning] Invalid target. Sender={Sender}, Target={Target}", senderId, targetConnectionId);
                return;
            }

            // 2. Gửi tín hiệu
            _logger.LogDebug("[Signal] From {Sender} -> {Target}", senderId, targetConnectionId);
            
            await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", senderId, signal);
        }

        // Gửi tín hiệu đến tất cả thành viên trong phòng (trừ người gửi)
        public async Task BroadcastSignal(string roomName, object signal)
            => await Clients.OthersInGroup(roomName).SendAsync("ReceiveBroadcastSignal", Context.ConnectionId, signal);

        // Gửi tin nhắn trong phòng họp
        public async Task SendMessage(int meetingId, string message)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");

            var roomName = meeting.RoomId;
            var userId = GetUserId().ToString();
            var userFullName = Context.User?.FindFirst(ClaimTypes.Name)?.Value 
                               ?? Context.User?.Identity?.Name
                               ?? $"User_{userId}";

            if (string.IsNullOrEmpty(roomName))
                 throw new HubException("RoomName không được null hoặc rỗng.");

            if (!_roomConnections.TryGetValue(roomName, out var roomDict) || !roomDict.ContainsKey(Context.ConnectionId))
                throw new HubException($"Phòng '{roomName}' không tồn tại.");
            
            await Clients.Group(roomName).SendAsync("ReceiveMessage", new
            {
                connectionId = Context.ConnectionId,
                userId,
                userFullName,
                message,
                sentAt = DateTime.Now
            });

            _logger.LogInformation("[HUB INFO] User {UserId} sent message to room {RoomName}: {Message}", userId, roomName, message);
        }

        // Thay đổi trạng thái media (âm thanh, video)
        public async Task ToggleMedia(string roomName, string type, bool enabled)
        {
            if (!_roomConnections.TryGetValue(roomName, out var roomDict) || !roomDict.ContainsKey(Context.ConnectionId))
                throw new HubException($"Bạn không tham gia phòng '{roomName}'");

            await Clients.OthersInGroup(roomName).SendAsync("UserMediaChanged", new
            {
                connectionId = Context.ConnectionId,
                type,
                enabled
            });

            _logger.LogInformation("[HUB INFO] User {ConnectionId} changed media {Type} to {Enabled} in room {RoomName}", 
                                    Context.ConnectionId, type, enabled, roomName);
        }

        // Chia sẻ màn hình
        public async Task StartScreenShare(int meetingId)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");

            if (!_roomConnections.TryGetValue(meeting.RoomId, out var roomDict) || !roomDict.ContainsKey(Context.ConnectionId))
                throw new HubException("Bạn không tham gia phòng để chia sẻ màn hình.");
            if (_roomScreenShares.ContainsKey(meeting.RoomId))
            {
                throw new HubException("Đang có người chia sẻ màn hình. Vui lòng đợi.");
            }

            if (!_roomScreenShares.TryAdd(meeting.RoomId, Context.ConnectionId))
            {
                throw new HubException("Không thể bắt đầu chia sẻ lúc này.");
            }
           
            await Clients.OthersInGroup(meeting.RoomId).SendAsync("ScreenShareStarted", new
            {
                connectionId = Context.ConnectionId,
                timestamp = DateTime.Now
            });

            var roomName = meeting.RoomId;
             if (_roomScreenShares.TryGetValue(roomName, out var currentSharerId))
                {
                    // Nếu người share không phải là chính mình thì chặn
                    if (currentSharerId != Context.ConnectionId)
                    {
                        throw new HubException("Người khác đang chia sẻ màn hình.");
                    }
                }
                else
                {
                    // Thử claim quyền share
                    if (!_roomScreenShares.TryAdd(roomName, Context.ConnectionId))
                    {
                        throw new HubException("Có người vừa bắt đầu chia sẻ. Vui lòng thử lại.");
                    }
                }

                // 2. Thông báo cho người khác
                await Clients.OthersInGroup(roomName).SendAsync("ScreenShareStarted", new
                {
                    connectionId = Context.ConnectionId,
                    timestamp = DateTime.Now
                });

            _logger.LogInformation("[HUB INFO] User {ConnectionId} started screen share in room {RoomName}", Context.ConnectionId, meeting.RoomId);
        }

        public async Task StopScreenShare(int meetingId)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");

            // 1. Kiểm tra user có trong phòng không
            if (!_roomConnections.TryGetValue(meeting.RoomId, out var roomDict) || !roomDict.ContainsKey(Context.ConnectionId))
                throw new HubException("Bạn không tham gia phòng.");

            // 2. Kiểm tra người gọi có phải là người đang share không
            if (_roomScreenShares.TryGetValue(meeting.RoomId, out var currentSharerId))
            {
                if (currentSharerId == Context.ConnectionId)
                {
                    // Xóa trạng thái
                    _roomScreenShares.TryRemove(meeting.RoomId, out _);
                    
                    // Chỉ gửi thông báo 1 lần tại đây
                    await Clients.OthersInGroup(meeting.RoomId).SendAsync("ScreenShareStopped", new
                    {
                        connectionId = Context.ConnectionId,
                        timestamp = DateTime.Now
                    });
                    
                    _logger.LogInformation("[HUB INFO] User {ConnectionId} stopped screen share", Context.ConnectionId);
                }
            }

            _logger.LogInformation("[HUB INFO] User {ConnectionId} stopped screen share in room {RoomName}", Context.ConnectionId, meeting.RoomId);
        }

        // Xử lý khi ngắt kết nối
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var roomEntry = _roomConnections.FirstOrDefault(r => r.Value.ContainsKey(connectionId));

            if (!roomEntry.Equals(default(KeyValuePair<string, ConcurrentDictionary<string, int>>)))
            {
                var roomName = roomEntry.Key;
                var roomDict = roomEntry.Value;

                if (roomDict.TryRemove(connectionId, out var userId))
                {
                    await Clients.Group(roomName).SendAsync("UserLeft", new
                    {
                        connectionId,
                        userId,
                        timestamp = DateTime.UtcNow
                    });

                    _logger.LogInformation("[HUB INFO] User {UserId} left room {RoomName}", userId, roomName);
                }

                if (_roomScreenShares.TryGetValue(roomName, out var sharerId) && sharerId == connectionId)
                {
                    _roomScreenShares.TryRemove(roomName, out _); // Xóa trạng thái share
                    
                    // Báo cho cả phòng biết để tắt màn hình share
                    await Clients.Group(roomName).SendAsync("ScreenShareStopped", new
                    {
                        connectionId = connectionId,
                        reason = "Disconnected",
                        timestamp = DateTime.Now
                    });
                    _logger.LogInformation("[HUB INFO] Sharer {ConnectionId} disconnected. Auto-stopped share.", connectionId);
                }

                if (roomDict.IsEmpty)
                    _roomConnections.TryRemove(roomName, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
