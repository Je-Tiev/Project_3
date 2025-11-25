namespace Paperless_Meeting.DTOs.Meeting;

public class MeetingListDto
{
    public int MeetingId { get; set; }
    public string Title { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; }
}
