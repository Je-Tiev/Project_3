using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Poll;
public class PollCreateRequestDto
{
    [Required]
    public int MeetingId { get; set; }
    [Required]
    public string Question { get; set; }
}