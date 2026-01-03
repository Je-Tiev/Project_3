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
                NoVotes = 0,
                UserHasVoted = false,
                UserChoice = null,
                Votes = new List<VoteDetailDto>()
            };
        }

        public async Task<PollDetailDto?> GetPollByIdAsync(int pollId, int currentUserId)
        {
           var poll = await _context.Polls
                .AsNoTracking()
                .Include(p => p.UserVotes)
                    .ThenInclude(uv => uv.User) // Join bảng User để lấy tên
                .FirstOrDefaultAsync(p => p.PollId == pollId);

            if (poll == null) return null;

            return MapToDetailDto(poll, currentUserId);
        }

        public async Task<List<PollDetailDto>> GetActivePollsByMeetingIdAsync(int meetingId, int currentUserId)
        {
            var activePolls = await _context.Polls
                .Where(p => p.MeetingId == meetingId && p.Status == Models.Poll.PollStatus.Open) // Chỉ lấy Open
                .Include(p => p.UserVotes) // Include để đếm số vote hiện tại
                .ThenInclude(uv => uv.User)
                .OrderBy(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return activePolls.Select(p => MapToDetailDto(p, currentUserId)).ToList();
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

        public async Task<List<PollDetailDto>> GetPollsByMeetingIdAsync(int meetingId, int currentUserId)
        {
            var polls = await _context.Polls
                .Where(p => p.MeetingId == meetingId)
                .Include(p => p.UserVotes)
                .ThenInclude(uv => uv.User)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return polls.Select(p => MapToDetailDto(p, currentUserId)).ToList();
        }

        // map dữ liệu và tính toán vote
        private PollDetailDto MapToDetailDto(Models.Poll poll, int currentUserId)
        {
            int yesCount = 0;
            int noCount = 0;
            bool userHasVoted = false;
            bool? userChoice = null;
            var voteDetails = new List<VoteDetailDto>();

            if (poll.UserVotes != null)
            {
                yesCount = poll.UserVotes.Count(v => v.Choice == true);
                noCount = poll.UserVotes.Count(v => v.Choice == false);

                var myVote = poll.UserVotes.FirstOrDefault(v => v.UserId == currentUserId);
                if (myVote != null)
                {
                    userHasVoted = true;
                    userChoice = myVote.Choice;
                }
                voteDetails = poll.UserVotes.Select(v => new VoteDetailDto
                {
                    UserId = v.UserId,
                    FullName = v.User != null ? v.User.FullName : $"User {v.UserId}", 
                    Choice = v.Choice,
                    VotedAt = v.VotedAt
                }).ToList();
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
                TotalVotes = yesCount + noCount,
                UserHasVoted = userHasVoted,
                UserChoice = userChoice,
                Votes = voteDetails
            };
        }
    }
}