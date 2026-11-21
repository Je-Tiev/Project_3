using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.MeetingLog;
using Paperless_Meeting.Repositories.Participant;

namespace Paperless_Meeting.Controllers.Meeting;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingLogsController : ControllerBase
{
    private readonly IMeetingLogService _logService;
    private readonly IMeetingService _meetingService;
    private readonly IParticipantService _participantService;

    public MeetingLogsController(
        IMeetingLogService logService,
        IMeetingService meetingService,
        IParticipantService participantService)
    {
        _logService = logService;
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
    public async Task<IActionResult> GetLogs(int meetingId)
    {
        var userId = GetUserId();
        var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
        if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

        var role = User.FindFirstValue(ClaimTypes.Role);
        var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
        var hasAccess = participants.Any(p => p.UserId == userId) || meeting.CreatedByUserId == userId || role == "Admin" || role == "SuperAdmin";
        if (!hasAccess)
        {
            return StatusCode(403, "Bạn không có quyền xem nhật ký cuộc họp này.");
        }

        var logs = await _logService.GetLogsByMeetingIdAsync(meetingId);
        return Ok(logs);
    }
}
