using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Meeting;

public class MeetingCreateRequestDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }
}