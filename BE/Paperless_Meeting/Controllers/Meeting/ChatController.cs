using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.Repositories.Chat;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.Participant;

namespace Paperless_Meeting.Controllers.Meeting;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IMeetingService _meetingService;
    private readonly IParticipantService _participantService;

    public ChatController(
        IChatService chatService,
        IMeetingService meetingService,
        IParticipantService participantService)
    {
        _chatService = chatService;
        _meetingService = meetingService;
        _participantService = participantService;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claim, out int userId)) return userId;
        throw new UnauthorizedAccessException("ID người dùng không hợp lệ.");
    }

    [HttpGet("meeting/{meetingId}")]
    public async Task<IActionResult> GetChatHistory(int meetingId)
    {
        var userId = GetUserId();
        var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
        if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

        var role = User.FindFirstValue(ClaimTypes.Role);
        var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
        var hasAccess = participants.Any(p => p.UserId == userId) || meeting.CreatedByUserId == userId || role == "Admin" || role == "SuperAdmin";
        if (!hasAccess)
        {
            return StatusCode(403, "Bạn không có quyền truy cập đoạn chat của cuộc họp này.");
        }

        var history = await _chatService.GetChatHistoryAsync(meetingId);
        return Ok(history);
    }
}
