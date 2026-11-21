using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.Repositories.Audio;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.Participant;

namespace Paperless_Meeting.Controllers.Meeting;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AudiosController : ControllerBase
{
    private readonly IAudioService _audioService;
    private readonly IMeetingService _meetingService;
    private readonly IParticipantService _participantService;

    public AudiosController(
        IAudioService audioService,
        IMeetingService meetingService,
        IParticipantService participantService)
    {
        _audioService = audioService;
        _meetingService = meetingService;
        _participantService = participantService;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claim, out int userId)) return userId;
        throw new UnauthorizedAccessException("ID người dùng không hợp lệ.");
    }

    [HttpPost("meeting/{meetingId}/upload")]
    [RequestSizeLimit(209715200)] // 200MB
    public async Task<IActionResult> UploadRecording(int meetingId, [FromForm] IFormFile file, [FromForm] float duration = 0)
    {
        var userId = GetUserId();
        var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
        if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

        var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
        var isParticipant = participants.Any(p => p.UserId == userId) || meeting.CreatedByUserId == userId;
        if (!isParticipant)
        {
            return StatusCode(403, "Bạn không có quyền tải bản ghi âm cho cuộc họp này.");
        }

        try
        {
            var audioDto = await _audioService.SaveAudioRecordingAsync(meetingId, userId, file, duration);
            return Ok(audioDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("meeting/{meetingId}")]
    public async Task<IActionResult> GetAudios(int meetingId)
    {
        var userId = GetUserId();
        var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
        if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

        var role = User.FindFirstValue(ClaimTypes.Role);
        var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
        var hasAccess = participants.Any(p => p.UserId == userId) || meeting.CreatedByUserId == userId || role == "Admin" || role == "SuperAdmin";
        if (!hasAccess)
        {
            return StatusCode(403, "Bạn không có quyền truy cập danh sách bản ghi âm.");
        }

        var list = await _audioService.GetAudiosByMeetingIdAsync(meetingId);
        return Ok(list);
    }

    [HttpGet("{audioId}/stream")]
    public async Task<IActionResult> StreamAudio(int audioId)
    {
        var userId = GetUserId();
        var fileInfo = await _audioService.GetAudioFilePathAsync(audioId, userId);
        if (fileInfo == null)
        {
            return NotFound("Không tìm thấy tệp ghi âm hoặc bạn không có quyền truy cập.");
        }

        return PhysicalFile(fileInfo.Value.filePath, fileInfo.Value.contentType, enableRangeProcessing: true);
    }
}
