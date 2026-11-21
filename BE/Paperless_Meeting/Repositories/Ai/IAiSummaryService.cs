namespace Paperless_Meeting.Repositories.Ai;

public interface IAiSummaryService
{
    Task<string> GenerateMeetingSummaryAsync(int meetingId);
    Task<string> TranscribeAudioAsync(int audioId);
}
