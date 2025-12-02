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

        // Lưu trữ connections trong mỗi room
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _roomConnections = new();
        public MeetingHub(IMeetingService meetingService, IParticipantService participantService)
        {
            _meetingService = meetingService;
            _participantService = participantService;
        }

        // Bước 1: Tham gia room
        public async Task JoinRoom(int meetingId)
        {
            //Kiểm qua quyền người dùng
            var userIdClaim = Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                  ?? Context.User?.FindFirst("nameidentifier")?.Value
                  ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                throw new HubException("Không xác thực người dùng.");

            var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
            if (!participants.Any(p => p.UserId == userId))
                throw new HubException("Bạn không có quyền tham gia cuộc họp.");

            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");

            var now = DateTime.UtcNow;
            if (now < meeting.StartTime || now > meeting.EndTime)
                throw new HubException("Hiện không phải thời gian diễn ra cuộc họp.");

            var roomName = meeting.RoomId;
            // Thêm vào group
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            // Thêm kết nối vào danh sách trong room
            var roomDict = _roomConnections.GetOrAdd(roomName, _ => new ConcurrentDictionary<string, int>());
            roomDict[Context.ConnectionId] = userId;

            //Lấy tên người dùng
            var userFullName = Context.User?.FindFirst(ClaimTypes.GivenName)?.Value
                               ?? Context.User?.Identity?.Name
                               ?? $"User_{userId}";
                
            // Thông báo cho mọi người khi có người tham gia
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

            // Gửi xác nhận đã tham gia room
            await Clients.Caller.SendAsync("JoinedRoom", new
            {
                roomName,
                meetingId,
                message = "Tham gia thành công!"
            });
        }

        // Bước 2: Gửi tín hiệu WebRTC đến người dùng khác
        public async Task SendSignal(string targetConnectionId, object signal)
        {
            await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", Context.ConnectionId, signal);
        }

        //Gửi tín hiệu đến các peer trong room
        public async Task BroadcastSignal(string roomName, object signal)
        {
            await Clients.OthersInGroup(roomName).SendAsync("ReceiveBroadcastSignal", Context.ConnectionId, signal);
        }

        // Bước 3: Gửi tin nhắn trong cuộc họp
        public async Task SendMessage(int meetingId, string message)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");
            var roomName = meeting.RoomId;
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            var userFullName = Context.User?.FindFirst(ClaimTypes.GivenName)?.Value
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
        public async Task ToggleMedia(string roomName,string type, bool enabled)
        {
            await Clients.OthersInGroup(roomName).SendAsync("UserMediaChanged", new
            {
                connectionId = Context.ConnectionId,
                type, // "microphone" | "video"
                enabled
            });
        }

        // Bắt đầu chia sẻ màn hình
        public async Task StartScreenShare(int meetingId)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");

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

            await Clients.OthersInGroup(roomName).SendAsync("ScreenShareStopped", new
            {
                connectionId = Context.ConnectionId,
                timestamp = DateTime.Now
            });
        }

        // Xử lý khi người dùng ngắt kết nối (rời phòng hoặc mất kết nối)
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var room in _roomConnections)
            {
                if (room.Value.TryRemove(Context.ConnectionId, out var userId))
                {
                    await Clients.Group(room.Key).SendAsync("UserLeft", new
                    {
                        connectionId = Context.ConnectionId,
                        userId,
                        timestamp = DateTime.UtcNow
                    });
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
