namespace Paperless_Meeting.DTOs.Note;

public class NoteDto
{
    public int NoteId { get; set; }
    public int MeetingId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
