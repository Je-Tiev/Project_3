using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Paperless_Meeting.DTOs.Participant;

namespace Paperless_Meeting.DTOs.Meeting;

public class MeetingCreateDto
{
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }
    
    // Optional list of participants to add when creating the meeting
    public List<ParticipantDto>? Participants { get; set; } = new List<ParticipantDto>();
}