using Paperless_Meeting.DTOs.Participant;

namespace Paperless_Meeting.Repositories.Participant;

public interface IParticipantService
{
    Task<List<UserViewDto>> GetMeetingParticipantsAsync(int meetingId);
    Task<bool> AddParticipantAsync(int meetingId, int userId, string role);
    Task<bool> RemoveParticipantAsync(int meetingId, int userId);
    Task<List<AddParticipantResultDto>> AddMultipleParticipantsAsync(int meetingId, List<ParticipantDto> participants);
    Task<bool> IsHostOfMeetingAsync(int meetingId, int userId);
}