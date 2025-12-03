using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.Participant;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace Paperless_Meeting.Hubs
{
    [Authorize]
    public class MeetingHub : Hub
    {
        private readonly IMeetingService _meetingService;
        private readonly IParticipantService _participantService;

        // Lưu trữ connections trong mỗi room: <RoomId, <ConnectionId, UserId>>
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _roomConnections = new();

        public MeetingHub(IMeetingService meetingService, IParticipantService participantService)
        {
            _meetingService = meetingService;
            _participantService = participantService;
        }

        // Helper để lấy User ID (Refactoring/Chuẩn hóa)
        private int GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? Context.User?.FindFirst("nameidentifier")?.Value; // Fallback
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            
            // Ném lỗi nếu không thể xác thực ID, vì Hub là [Authorize] nên điều này không nên xảy ra.
            throw new HubException("User ID claim not found or invalid.");
        }

        // Bước 1: Tham gia room
       public async Task JoinRoom(int meetingId)
{
    Console.WriteLine($"[HUB INFO] JoinRoom called for meeting {meetingId} by connection {Context.ConnectionId}");
    try
    {
        // Lấy UserId từ token
        var userId = GetUserId();
        Console.WriteLine($"[DEBUG] UserId from token: {userId}");

        // Lấy thông tin meeting
        var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
        if (meeting == null)
        {
            Console.WriteLine($"[DEBUG] Meeting {meetingId} not found");
            throw new HubException("Không tìm thấy cuộc họp.");
        }

        Console.WriteLine($"[DEBUG] Meeting RoomId: {meeting.RoomId}, StartTime: {meeting.StartTime}, EndTime: {meeting.EndTime}");

        // Kiểm tra thời gian họp
        var now = DateTime.UtcNow;
        if (now < meeting.StartTime || now > meeting.EndTime)
        {
            Console.WriteLine($"[DEBUG] Current time {now} is outside meeting time");
            throw new HubException("Hiện không phải thời gian diễn ra cuộc họp.");
        }

        // Lấy danh sách participants
        var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
        if (participants == null || !participants.Any(p => p.UserId == userId))
        {
            var participantIds = participants != null ? string.Join(", ", participants.Select(p => p.UserId)) : "null";
            Console.WriteLine($"[DEBUG] Participants: {participantIds}");
            throw new HubException("Bạn không có quyền tham gia cuộc họp này.");
        }

        var roomName = meeting.RoomId;
        if (string.IsNullOrEmpty(roomName))
        {
            Console.WriteLine("[DEBUG] Meeting RoomId is null or empty");
            throw new HubException("Cuộc họp chưa được cấu hình RoomId.");
        }

        // Thêm vào group SignalR
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

        var roomDict = _roomConnections.GetOrAdd(roomName, _ => new ConcurrentDictionary<string, int>());
        roomDict[Context.ConnectionId] = userId;

        // Gửi thông tin UserJoined cho mọi người
        var userFullName = Context.User?.FindFirst(ClaimTypes.Name)?.Value
                           ?? Context.User?.FindFirst(ClaimTypes.GivenName)?.Value
                           ?? $"User_{userId}";
        await Clients.OthersInGroup(roomName).SendAsync("UserJoined", new
        {
            userId,
            connectionId = Context.ConnectionId,
            fullName = userFullName
        });

        // Gửi danh sách người tham gia hiện tại cho client mới
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

        // Gửi xác nhận tham gia thành công
        await Clients.Caller.SendAsync("JoinedRoom", new
        {
            roomName,
            meetingId,
            message = "Tham gia thành công!"
        });

        Console.WriteLine($"[HUB INFO] User {userId} successfully joined room {roomName}");
        
    }
    catch (HubException ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[HUB BUSINESS ERROR] MeetingId: {meetingId}, Message: {ex.Message}");
        Console.ResetColor();
        throw;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[HUB FATAL ERROR] MeetingId: {meetingId}, Type: {ex.GetType().Name}, Message: {ex.Message}");
        Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
        Console.ResetColor();
        throw new HubException("Lỗi không mong muốn khi tham gia cuộc họp. Vui lòng kiểm tra log server.");
    }
}

        // --- Các phương thức tín hiệu (WebRTC) ---
        public async Task SendSignal(string targetConnectionId, object signal)
        {
            // Cân nhắc thêm kiểm tra người gọi và người nhận có ở cùng một room không
            await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", Context.ConnectionId, signal);
        }

        public async Task BroadcastSignal(string roomName, object signal)
        {
            // Cân nhắc thêm kiểm tra người gọi có ở trong roomName không
            await Clients.OthersInGroup(roomName).SendAsync("ReceiveBroadcastSignal", Context.ConnectionId, signal);
        }

        // Bước 3: Gửi tin nhắn trong cuộc họp
        public async Task SendMessage(int meetingId, string message)
        {
             // Nên kiểm tra người dùng có quyền gửi tin nhắn (tức là đã join room) không
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");
                
            var roomName = meeting.RoomId;
            var userId = GetUserId().ToString(); // Lấy ID và chuyển thành string
            var userFullName = Context.User?.FindFirst(ClaimTypes.Name)?.Value 
                               ?? Context.User?.Identity?.Name
                               ?? $"User_{userId}";
                               
            await Clients.Group(roomName).SendAsync("ReceiveMessage", new
            {
                connectionId = Context.ConnectionId,
                userId,
                userFullName,
                message,
                sentAt = DateTime.Now
            });
        }

        // Bước 4: Bật/Tắt micro hoặc camera
        public async Task ToggleMedia(string roomName, string type, bool enabled)
        {
            try
            {
                // ⭐ ĐIỀU CHỈNH: Xác minh người dùng đang trong phòng này
                if (!_roomConnections.TryGetValue(roomName, out var roomDict) || !roomDict.ContainsKey(Context.ConnectionId))
                {
                    throw new HubException($"Bạn không tham gia phòng '{roomName}'");
                }
                
                // Gửi thông báo đến những người khác trong nhóm
                await Clients.OthersInGroup(roomName).SendAsync("UserMediaChanged", new
                {
                    connectionId = Context.ConnectionId,
                    type, // "microphone" | "video"
                    enabled
                });
            }
            catch (HubException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // ⭐ ĐIỀU CHỈNH: Thêm xử lý lỗi để bắt lỗi client 'ToggleMedia' đã báo cáo
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[HUB FATAL ERROR - ToggleMedia] Room: {roomName}, Type: {type}, Message: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                Console.ResetColor();
                throw new HubException("Lỗi không mong muốn khi ToggleMedia. Vui lòng kiểm tra log chi tiết."); 
            }
        }

        // Bắt đầu chia sẻ màn hình
        public async Task StartScreenShare(int meetingId)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");
            
             // ⭐ ĐIỀU CHỈNH: Xác minh người dùng đang trong phòng này
            if (!_roomConnections.TryGetValue(meeting.RoomId, out var roomDict) || !roomDict.ContainsKey(Context.ConnectionId))
            {
                throw new HubException($"Bạn không tham gia phòng để chia sẻ màn hình.");
            }

            await Clients.OthersInGroup(meeting.RoomId).SendAsync("ScreenShareStarted", new
            {
                connectionId = Context.ConnectionId,
                timestamp = DateTime.Now
            });
        }
        
        // Dừng chia sẻ màn hình
        public async Task StopScreenShare(int meetingId)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");
            
            var roomName = meeting.RoomId;
            
            // ⭐ ĐIỀU CHỈNH: Xác minh người dùng đang trong phòng này
            if (!_roomConnections.TryGetValue(roomName, out var roomDict) || !roomDict.ContainsKey(Context.ConnectionId))
            {
                throw new HubException($"Bạn không tham gia phòng để dừng chia sẻ màn hình.");
            }

            await Clients.OthersInGroup(roomName).SendAsync("ScreenShareStopped", new
            {
                connectionId = Context.ConnectionId,
                timestamp = DateTime.Now
            });
        }

        // Xử lý khi người dùng ngắt kết nối (rời phòng hoặc mất kết nối)
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            
            // ⭐ ĐIỀU CHỈNH: Dùng linq để tìm và dọn dẹp kết nối hiệu quả hơn
            var roomEntry = _roomConnections.FirstOrDefault(room => room.Value.ContainsKey(connectionId));

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
                }
                
                // Nếu phòng không còn kết nối nào, có thể dọn dẹp entry của phòng khỏi _roomConnections
                if (roomDict.IsEmpty)
                {
                     _roomConnections.TryRemove(roomName, out _);
                }
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}