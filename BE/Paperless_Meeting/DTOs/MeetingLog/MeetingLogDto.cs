namespace Paperless_Meeting.DTOs.MeetingLog;

public class MeetingLogDto
{
    public int LogId { get; set; }
    public int MeetingId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime ActionTime { get; set; }
}
