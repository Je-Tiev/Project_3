using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Note;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.Note;
using Paperless_Meeting.Repositories.Participant;

namespace Paperless_Meeting.Controllers.Meeting;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;
    private readonly IMeetingService _meetingService;
    private readonly IParticipantService _participantService;

    public NotesController(
        INoteService noteService,
        IMeetingService meetingService,
        IParticipantService participantService)
    {
        _noteService = noteService;
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
    public async Task<IActionResult> GetMyNote(int meetingId)
    {
        var userId = GetUserId();

        // Kiểm tra quyền tham gia cuộc họp
        var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
        if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

        var note = await _noteService.GetNoteByMeetingAndUserAsync(meetingId, userId);
        if (note == null)
        {
            return Ok(new NoteDto
            {
                NoteId = 0,
                MeetingId = meetingId,
                UserId = userId,
                Content = "",
                CreatedAt = DateTime.UtcNow
            });
        }

        return Ok(note);
    }

    [HttpPost("meeting/{meetingId}")]
    public async Task<IActionResult> SaveNote(int meetingId, [FromBody] NoteUpsertDto dto)
    {
        var userId = GetUserId();

        var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
        if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

        var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
        var isParticipant = participants.Any(p => p.UserId == userId) || meeting.CreatedByUserId == userId;
        if (!isParticipant)
        {
            return StatusCode(403, "Bạn không phải thành viên tham gia cuộc họp này.");
        }

        var savedNote = await _noteService.UpsertNoteAsync(meetingId, userId, dto.Content);
        return Ok(savedNote);
    }
}
