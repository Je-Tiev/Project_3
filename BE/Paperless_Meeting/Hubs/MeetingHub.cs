using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.Participant;
using System.Security.Claims;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Paperless_Meeting.Hubs
{
    [Authorize]
    public class MeetingHub : Hub
    {
        private readonly IMeetingService _meetingService;
        private readonly IParticipantService _participantService;
        private readonly ILogger<MeetingHub> _logger;
        // <RoomName, <ConnectionId, UserId>>
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _roomConnections = new();

        public MeetingHub(IMeetingService meetingService,
                          IParticipantService participantService,
                          ILogger<MeetingHub> logger)
        {
            _meetingService = meetingService;
            _participantService = participantService;
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

        public async Task JoinRoom(int meetingId)
        {
            _logger.LogInformation("[HUB INFO] JoinRoom called: MeetingId={MeetingId}, Connection={ConnectionId}", meetingId, Context.ConnectionId);
            try
            {
                var userId = GetUserId();
                _logger.LogDebug("[DEBUG] UserId from token: {UserId}", userId);

                var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
                if (meeting == null)
                {
                    _logger.LogWarning("[DEBUG] Meeting {MeetingId} not found", meetingId);
                    throw new HubException("Không tìm thấy cuộc họp.");
                }

                _logger.LogDebug("[DEBUG] Meeting RoomId={RoomId}, Start={Start}, End={End}", meeting.RoomId, meeting.StartTime, meeting.EndTime);

                if (string.IsNullOrEmpty(meeting.RoomId))
                {
                    _logger.LogWarning("[DEBUG] Meeting RoomId is null or empty");
                    throw new HubException("Cuộc họp chưa được cấu hình RoomId.");
                }

                var now = DateTime.UtcNow;
                if (now < meeting.StartTime || now > meeting.EndTime)
                {
                    _logger.LogWarning("[DEBUG] Current UTC time {Now} is outside meeting time", now);
                    throw new HubException("Hiện không phải thời gian diễn ra cuộc họp.");
                }

                var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
                if (participants == null || !participants.Any(p => p.UserId == userId))
                {
                    var participantIds = participants != null ? string.Join(", ", participants.Select(p => p.UserId)) : "null";
                    _logger.LogWarning("[DEBUG] Participants: {ParticipantIds}", participantIds);
                    throw new HubException("Bạn không có quyền tham gia cuộc họp này.");
                }

                var roomName = meeting.RoomId;
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

                var roomDict = _roomConnections.GetOrAdd(roomName, _ => new ConcurrentDictionary<string, int>());
                roomDict[Context.ConnectionId] = userId;

                var userFullName = Context.User?.FindFirst(ClaimTypes.Name)?.Value
                                   ?? Context.User?.FindFirst(ClaimTypes.GivenName)?.Value
                                   ?? $"User_{userId}";

                await Clients.OthersInGroup(roomName).SendAsync("UserJoined", new
                {
                    userId,
                    connectionId = Context.ConnectionId,
                    fullName = userFullName
                });

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

        public async Task BroadcastSignal(string roomName, object signal)
            => await Clients.OthersInGroup(roomName).SendAsync("ReceiveBroadcastSignal", Context.ConnectionId, signal);

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

        public async Task StartScreenShare(int meetingId)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");

            if (!_roomConnections.TryGetValue(meeting.RoomId, out var roomDict) || !roomDict.ContainsKey(Context.ConnectionId))
                throw new HubException("Bạn không tham gia phòng để chia sẻ màn hình.");

            await Clients.OthersInGroup(meeting.RoomId).SendAsync("ScreenShareStarted", new
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

            if (!_roomConnections.TryGetValue(meeting.RoomId, out var roomDict) || !roomDict.ContainsKey(Context.ConnectionId))
                throw new HubException("Bạn không tham gia phòng để dừng chia sẻ màn hình.");

            await Clients.OthersInGroup(meeting.RoomId).SendAsync("ScreenShareStopped", new
            {
                connectionId = Context.ConnectionId,
                timestamp = DateTime.Now
            });

            _logger.LogInformation("[HUB INFO] User {ConnectionId} stopped screen share in room {RoomName}", Context.ConnectionId, meeting.RoomId);
        }

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

                if (roomDict.IsEmpty)
                    _roomConnections.TryRemove(roomName, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
