using Paperless_Meeting.DTOs.Conclusion;

namespace Paperless_Meeting.Repositories.Conclusion;

public interface IConclusionService
{
    Task<ConclusionDto?> GetConclusionByMeetingIdAsync(int meetingId);
    Task<ConclusionDto> UpsertConclusionAsync(int meetingId, int userId, ConclusionUpsertDto dto);
    Task<ConclusionDto?> SignConclusionAsync(int meetingId, int userId, string userName);
}
