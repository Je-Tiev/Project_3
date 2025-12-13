using Paperless_Meeting.DTOs.Poll;

namespace Paperless_Meeting.Services.Poll
{
    public interface IPollService
    {
        Task<PollDetailDto> CreatePollAsync(PollCreateRequestDto request, int creatorId);
        Task<PollDetailDto?> GetPollByIdAsync(int pollId);
        Task<bool> SubmitVoteAsync(int pollId, int userId, bool choice);
        Task<bool> HasUserVotedAsync(int pollId, int userId);
        Task<bool> ClosePollAsync(int pollId);
        
        Task<List<PollDetailDto>> GetPollsByMeetingIdAsync(int meetingId);
    }
}