using Paperless_Meeting.DTOs.Note;

namespace Paperless_Meeting.Repositories.Note;

public interface INoteService
{
    Task<NoteDto?> GetNoteByMeetingAndUserAsync(int meetingId, int userId);
    Task<NoteDto> UpsertNoteAsync(int meetingId, int userId, string content);
}
