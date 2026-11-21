using Paperless_Meeting.DTOs.MeetingLog;

namespace Paperless_Meeting.Repositories.MeetingLog;

public interface IMeetingLogService
{
    Task LogActionAsync(int meetingId, int userId, string action);
    Task<List<MeetingLogDto>> GetLogsByMeetingIdAsync(int meetingId);
}
