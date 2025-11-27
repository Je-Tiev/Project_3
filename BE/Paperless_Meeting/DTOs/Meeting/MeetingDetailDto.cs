using Paperless_Meeting.Models;
using Paperless_Meeting.DTOs.Participant;
namespace Paperless_Meeting.DTOs.Meeting;

public class MeetingDetailDto
{
    public int MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public Models.Meeting.MeetingStatus Status { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ParticipantDto> Participants { get; set; } = new List<ParticipantDto>();
}