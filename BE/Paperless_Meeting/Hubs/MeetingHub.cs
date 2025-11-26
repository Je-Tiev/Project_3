using Microsoft.AspNetCore.SignalR;
using Paperless_Meeting.Interfaces.Meeting;
using System.Security.Claims;
using System.Linq;
using System;

namespace Paperless_Meeting.Hubs
{
    public class MeetingHub : Hub
    {
        private readonly IMeetingService _meetingService;

        public MeetingHub(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        // Join by meetingId (server will validate participant list)
        public async Task JoinRoom(int meetingId)
        {
            var userFullName = Context.User?.FindFirst(ClaimTypes.GivenName)?.Value
                ?? Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(userFullName))
                throw new HubException("Không xác thực tên người dùng.");

            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                throw new HubException("Không tìm thấy cuộc họp.");

            var allowed = meeting.Participants != null && meeting.Participants.Any(p =>
                !string.IsNullOrWhiteSpace(p.FullName) &&
                string.Equals(p.FullName.Trim(), userFullName.Trim(), StringComparison.OrdinalIgnoreCase)
            );

            if (!allowed)
                throw new HubException("Bạn không có quyền tham gia cuộc họp này.");

            var roomName = $"meeting-{meetingId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            await Clients.OthersInGroup(roomName).SendAsync("UserJoined", Context.ConnectionId);
        }

        public async Task SendSignal(string roomId, string targetId, object signal)
        {
            await Clients.Client(targetId).SendAsync("ReceiveSignal", Context.ConnectionId, signal);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync("UserLeft", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}