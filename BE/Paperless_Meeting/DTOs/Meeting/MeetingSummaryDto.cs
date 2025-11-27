using Paperless_Meeting.Models;

namespace Paperless_Meeting.DTOs.Meeting;

public class MeetingSummaryDto
{
    public int MeetingId { get; set; }
    public string Title { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public Models.Meeting.MeetingStatus Status { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
}