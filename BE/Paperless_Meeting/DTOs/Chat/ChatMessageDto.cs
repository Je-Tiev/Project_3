namespace Paperless_Meeting.DTOs.Chat;

public class ChatMessageDto
{
    public int ChatId { get; set; }
    public int MeetingId { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
