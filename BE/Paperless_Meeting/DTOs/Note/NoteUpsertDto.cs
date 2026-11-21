using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Note;

public class NoteUpsertDto
{
    public string Content { get; set; } = string.Empty;
}
