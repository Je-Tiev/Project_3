namespace Paperless_Meeting.DTOs.Audio;

public class AudioDto
{
    public int AudioId { get; set; }
    public int MeetingId { get; set; }
    public int UserId { get; set; }
    public string SpeakerName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public float Duration { get; set; }
    public string? TranscribedText { get; set; }
    public DateTime CreatedAt { get; set; }
}
