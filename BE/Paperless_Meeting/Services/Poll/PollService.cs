using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Poll;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Poll;

namespace Paperless_Meeting.Services.Poll
{
    public class PollService : IPollService
    {
        private readonly PaperlessMeetingDbContext _context;

        public PollService(PaperlessMeetingDbContext context)
        {
            _context = context;
        }

        public async Task<PollDetailDto> CreatePollAsync(PollCreateRequestDto request, int creatorId)
        {
            var poll = new Models.Poll
            {
                MeetingId = request.MeetingId,
                Question = request.Question,
                VoteByUserId = creatorId,
                Status = Models.Poll.PollStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            return new PollDetailDto
            {
                PollId = poll.PollId,
                MeetingId = poll.MeetingId,
                Question = poll.Question,
                Status = poll.Status.ToString(),
                CreatedAt = poll.CreatedAt,
                TotalVotes = 0,
                YesVotes = 0,
                NoVotes = 0
            };
        }

        public async Task<PollDetailDto?> GetPollByIdAsync(int pollId)
        {
            return await _context.Polls
                .AsNoTracking()
                .Where(p => p.PollId == pollId)
                .Select(p => new PollDetailDto
                {
                    PollId = p.PollId,
                    MeetingId = p.MeetingId,
                    Question = p.Question,
                    Status = p.Status.ToString(), 
                    CreatedAt = p.CreatedAt,
                    YesVotes = p.UserVotes.Count(v => v.Choice == true), 
                    NoVotes = p.UserVotes.Count(v => v.Choice == false),
                    TotalVotes = p.UserVotes.Count()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SubmitVoteAsync(int pollId, int userId, bool choice)
        {
            // Kiểm tra Poll có tồn tại và đang mở không
            var poll = await _context.Polls.FindAsync(pollId);
            if (poll == null || poll.Status == Models.Poll.PollStatus.Closed)
                return false;

            // Tạo bản ghi UserVote
            var userVote = new UserVote
            {
                PollId = pollId,
                UserId = userId,
                Choice = choice,
                VotedAt = DateTime.UtcNow
            };

            _context.UserVotes.Add(userVote);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> HasUserVotedAsync(int pollId, int userId)
        {
            return await _context.UserVotes
                .AnyAsync(uv => uv.PollId == pollId && uv.UserId == userId);
        }

        public async Task<bool> ClosePollAsync(int pollId)
        {
            var poll = await _context.Polls.FindAsync(pollId);
            if (poll == null) return false;

            poll.Status = Models.Poll.PollStatus.Closed;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PollDetailDto>> GetPollsByMeetingIdAsync(int meetingId)
        {
            var polls = await _context.Polls
                .Where(p => p.MeetingId == meetingId)
                .Include(p => p.UserVotes)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return polls.Select(p => MapToDetailDto(p)).ToList();
        }

        // map dữ liệu và tính toán vote
        private PollDetailDto MapToDetailDto(Models.Poll poll)
        {
            int yesCount = 0;
            int noCount = 0;

            if (poll.UserVotes != null)
            {
                yesCount = poll.UserVotes.Count(v => v.Choice == true);
                noCount = poll.UserVotes.Count(v => v.Choice == false);
            }

            return new PollDetailDto
            {
                PollId = poll.PollId,
                MeetingId = poll.MeetingId,
                Question = poll.Question,
                Status = poll.Status.ToString(),
                CreatedAt = poll.CreatedAt,
                YesVotes = yesCount,
                NoVotes = noCount,
                TotalVotes = yesCount + noCount
            };
        }
    }
}