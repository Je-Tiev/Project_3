using Paperless_Meeting.DTOs.Meeting;

namespace Paperless_Meeting.Interfaces.Meeting;

public interface IMeetingService
{
    Task<IEnumerable<MeetingSummaryDto>> GetAllMeetingsAsync();
    Task<MeetingDetailDto?> GetMeetingByIdAsync(int meetingId);
    Task<MeetingDetailDto> CreateMeetingAsync(MeetingCreateDto createDto, int creatorId); 
    Task<bool> UpdateMeetingAsync(int meetingId, MeetingCreateDto updateDto);
    Task<bool> DeleteMeetingAsync(int meetingId);
}