using Paperless_Meeting.DTOs.Meeting;

namespace Paperless_Meeting.Repositories.Meeting;

public interface IMeetingService
{
    Task<MeetingDetailDto> CreateMeetingAsync(MeetingCreateDto createDto, int creatorId);
    Task<IEnumerable<MeetingSummaryDto>> GetAllMeetingsAsync();
    Task<MeetingDetailDto?> GetMeetingByIdAsync(int meetingId);
    Task<List<MeetingListDto>> GetMeetingsByUserAsync(int userId);
    Task<bool> UpdateMeetingAsync(int meetingId, MeetingUpdateDto updateDto);
    Task<bool> ExtendMeetingAsync(int meetingId, int userId);
    Task<bool> DeleteMeetingAsync(int meetingId);
}