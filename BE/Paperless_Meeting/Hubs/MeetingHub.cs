using Microsoft.AspNetCore.SignalR;
using Paperless_Meeting.Repositories.Meeting;
using System.Security.Claims;
using System.Linq;

namespace Paperless_Meeting.Hubs
{
    public class MeetingHub : Hub
    {
        private readonly IMeetingService _meetingService;

        public MeetingHub(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        // ✅ CHỨC NĂNG THAM GIA PHÒNG HỌP (ĐÃ SỬA)
        public async Task JoinRoom(int meetingId)
        {
            // 1. ✅ LẤY USER ID (CHÍNH XÁC)
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                throw new HubException("Không xác thực ID người dùng.");

            // 2. ✅ LẤY THÔNG TIN USER
            var userFullName = Context.User?.FindFirst(ClaimTypes.GivenName)?.Value
                ?? Context.User?.FindFirst(ClaimTypes.Name)?.Value
                ?? $"User_{userId}";

            // 3. ✅ KIỂM TRA CUỘC HỌP TỒN TẠI
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");

            // 4. ✅ KIỂM TRA QUYỀN (DÙNG USERID)
            var isParticipant = meeting.Participants?.Any(p => p.UserId == userId) == true;
            if (!isParticipant)
                throw new HubException("Bạn không có quyền tham gia cuộc họp này.");

            // 5. ✅ KIỂM TRA THỜI GIAN (VIỆT NAM)
            var nowUtc = DateTime.UtcNow;
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vnTimeZone);
            var startTime = (DateTime)meeting.StartTime!;
            var endTime = (DateTime)meeting.EndTime!;

            if (nowVn < startTime)
                throw new HubException($"Cuộc họp chưa bắt đầu. Bắt đầu lúc: {startTime:HH:mm dd/MM/yyyy}");
            if (nowVn > endTime)
                throw new HubException($"Cuộc họp đã kết thúc. Kết thúc lúc: {endTime:HH:mm dd/MM/yyyy}");

            // 6. ✅ THAM GIA GROUP
            var roomName = $"meeting-{meetingId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            // 7. ✅ THÔNG BÁO CHO NHÓM (FULL INFO)
            await Clients.OthersInGroup(roomName).SendAsync("UserJoined", new
            {
                connectionId = Context.ConnectionId,
                userId = userId,
                fullName = userFullName,
                joinedAt = DateTime.UtcNow
            });

            // 8. ✅ GỬI DANH SÁCH USER CHO NGƯỜI MỚI JOIN
            await Clients.Caller.SendAsync("UserJoinedSuccessfully", new
            {
                message = "Tham gia thành công!",
                roomName,
                userId
            });

            // Log (optional)
            Console.WriteLine($"User {userFullName} (ID: {userId}) joined meeting {meetingId}");
        }

        // ✅ GỬI SIGNAL (WebRTC)
        public async Task SendSignal(string roomId, string targetConnectionId, object signal)
        {
            await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", 
                Context.ConnectionId, signal);
        }

        // ✅ BROADCAST MESSAGE
        public async Task SendMessage(int meetingId, string message)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.Identity?.Name ?? "Anonymous";
            
            var roomName = $"meeting-{meetingId}";
            await Clients.Group(roomName).SendAsync("ReceiveMessage", new
            {
                userId = userIdClaim,
                userName,
                message,
                timestamp = DateTime.UtcNow
            });
        }

        // ✅ TOGGLE MICROPHONE/VIDEO
        public async Task ToggleMedia(string type, bool enabled)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.Identity?.Name ?? "Anonymous";
            
            await Clients.Others.SendAsync("UserMediaChanged", new
            {
                connectionId = Context.ConnectionId,
                userId = userIdClaim,
                userName,
                type, // "microphone" | "video"
                enabled
            });
        }

        // ✅ DISCONNECT
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Thông báo cho tất cả rooms user đang tham gia
            await Clients.All.SendAsync("UserLeft", new
            {
                connectionId = Context.ConnectionId,
                userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                leftAt = DateTime.UtcNow
            });
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}