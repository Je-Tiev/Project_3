using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Paperless_Meeting.DTOs.Conclusion;
using Paperless_Meeting.Hubs;
using Paperless_Meeting.Repositories.Conclusion;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.Participant;

namespace Paperless_Meeting.Controllers.Meeting;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConclusionsController : ControllerBase
{
    private readonly IConclusionService _conclusionService;
    private readonly IMeetingService _meetingService;
    private readonly IParticipantService _participantService;
    private readonly IHubContext<MeetingHub> _hubContext;

    public ConclusionsController(
        IConclusionService conclusionService,
        IMeetingService meetingService,
        IParticipantService participantService,
        IHubContext<MeetingHub> hubContext)
    {
        _conclusionService = conclusionService;
        _meetingService = meetingService;
        _participantService = participantService;
        _hubContext = hubContext;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claim, out int userId)) return userId;
        throw new UnauthorizedAccessException("ID người dùng không hợp lệ.");
    }

    [HttpGet("meeting/{meetingId}")]
    public async Task<IActionResult> GetConclusion(int meetingId)
    {
        var conclusion = await _conclusionService.GetConclusionByMeetingIdAsync(meetingId);
        if (conclusion == null)
        {
            return Ok(new ConclusionDto
            {
                ConclusionId = 0,
                MeetingId = meetingId,
                Summary = "",
                Status = "Draft",
                Signatures = new List<ConclusionSignatureDto>(),
                CreatedAt = DateTime.UtcNow
            });
        }
        return Ok(conclusion);
    }

    [HttpPost("meeting/{meetingId}")]
    public async Task<IActionResult> UpsertConclusion(int meetingId, [FromBody] ConclusionUpsertDto dto)
    {
        var userId = GetUserId();
        var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
        if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

        // Quyền chỉnh sửa kết luận: Host hoặc Admin/SuperAdmin
        var role = User.FindFirstValue(ClaimTypes.Role);
        var isHostOrAdmin = meeting.CreatedByUserId == userId || role == "Admin" || role == "SuperAdmin";
        if (!isHostOrAdmin)
        {
            return StatusCode(403, "Chỉ chủ trì hoặc quản trị viên mới có quyền cập nhật biên bản kết luận.");
        }

        var result = await _conclusionService.UpsertConclusionAsync(meetingId, userId, dto);

        // Phát tín hiệu SignalR cập nhật kết luận cho các thành viên trong phòng
        if (!string.IsNullOrEmpty(meeting.RoomId))
        {
            await _hubContext.Clients.Group(meeting.RoomId).SendAsync("ConclusionUpdated", result);
        }

        return Ok(result);
    }

    [HttpPost("meeting/{meetingId}/sign")]
    public async Task<IActionResult> SignConclusion(int meetingId)
    {
        var userId = GetUserId();
        var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
        if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

        var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
        var isParticipant = participants.Any(p => p.UserId == userId) || meeting.CreatedByUserId == userId;
        if (!isParticipant)
        {
            return StatusCode(403, "Bạn không phải thành viên tham gia cuộc họp này để ký.");
        }

        var userName = User.FindFirstValue(ClaimTypes.Name) ?? $"User_{userId}";
        var result = await _conclusionService.SignConclusionAsync(meetingId, userId, userName);

        if (result == null) return NotFound("Chưa có biên bản kết luận nào để ký.");

        // Báo cho mọi người đã có người ký
        if (!string.IsNullOrEmpty(meeting.RoomId))
        {
            await _hubContext.Clients.Group(meeting.RoomId).SendAsync("ConclusionSigned", result);
        }

        return Ok(result);
    }
}
