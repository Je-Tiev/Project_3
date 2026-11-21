using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Poll;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class PollConcurrencyTests : IDisposable
    {
        private readonly SqliteConnection _sqliteConnection;
        private readonly DbContextOptions<PaperlessMeetingDbContext> _dbOptions;

        public PollConcurrencyTests()
        {
            // Sử dụng SQLite In-Memory Connection để ép buộc kiểm tra Unique Index tại cấp độ RDBMS
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            _sqliteConnection.Open();

            _dbOptions = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseSqlite(_sqliteConnection)
                .Options;

            using var context = new PaperlessMeetingDbContext(_dbOptions);
            context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _sqliteConnection.Dispose();
        }

        private async Task SeedPollDataAsync(PaperlessMeetingDbContext context)
        {
            // Seed Department
            context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "Ban Giám đốc" });

            // Seed Users
            context.Users.AddRange(
                new User { UserId = 1, Username = "organizer", FullName = "Organizer", Email = "org@hust.edu.vn", PasswordHash = "h", DepartmentId = 1, Role = User.UserRole.User },
                new User { UserId = 2, Username = "voter1", FullName = "Voter One", Email = "voter1@hust.edu.vn", PasswordHash = "h", DepartmentId = 1, Role = User.UserRole.User },
                new User { UserId = 3, Username = "voter2", FullName = "Voter Two", Email = "voter2@hust.edu.vn", PasswordHash = "h", DepartmentId = 1, Role = User.UserRole.User }
            );

            // Seed Meeting
            context.Meetings.Add(new Meeting
            {
                MeetingId = 1,
                Title = "Meeting with Poll",
                RoomId = "ROOM-1",
                Location = "Room 101",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2),
                CreatedByUserId = 1,
                Status = Meeting.MeetingStatus.InProgress
            });

            // Seed Open Poll
            context.Polls.Add(new Poll
            {
                PollId = 1,
                MeetingId = 1,
                Question = "Thông qua kế hoạch ngân sách năm 2027?",
                Status = Poll.PollStatus.Open,
                CreatedAt = DateTime.UtcNow,
                VoteByUserId = 1
            });

            // Seed Closed Poll
            context.Polls.Add(new Poll
            {
                PollId = 2,
                MeetingId = 1,
                Question = "Cuộc thăm dò đã đóng",
                Status = Poll.PollStatus.Closed,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                VoteByUserId = 1
            });

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task SubmitVoteAsync_FirstTimeVote_ReturnsTrue()
        {
            // Arrange
            using var context = new PaperlessMeetingDbContext(_dbOptions);
            await SeedPollDataAsync(context);
            var service = new PollService(context);

            // Act: Lần biểu quyết đầu tiên
            var result = await service.SubmitVoteAsync(pollId: 1, userId: 2, choice: true);

            // Assert
            result.Should().BeTrue();
            var vote = await context.UserVotes.FirstOrDefaultAsync(v => v.PollId == 1 && v.UserId == 2);
            vote.Should().NotBeNull();
            vote!.Choice.Should().BeTrue();
        }

        [Fact]
        public async Task SubmitVoteAsync_SecondTimeVote_FailsUniqueConstraint_ReturnsFalse()
        {
            // Arrange
            using var context = new PaperlessMeetingDbContext(_dbOptions);
            await SeedPollDataAsync(context);
            var service = new PollService(context);

            // Act 1: Bỏ phiếu lần 1 -> Thành công
            var firstVote = await service.SubmitVoteAsync(pollId: 1, userId: 2, choice: true);
            firstVote.Should().BeTrue();

            // Act 2: Cố tình bỏ phiếu lần 2 (Double Voting)
            var secondVote = await service.SubmitVoteAsync(pollId: 1, userId: 2, choice: false);

            // Assert: Hệ thống bắt được vi phạm Unique Index (PollId, UserId) và từ chối biểu quyết
            secondVote.Should().BeFalse();

            // Cơ sở dữ liệu vẫn chỉ lưu đúng 1 phiếu của user này
            var voteCount = await context.UserVotes.CountAsync(v => v.PollId == 1 && v.UserId == 2);
            voteCount.Should().Be(1);
        }

        [Fact]
        public async Task SubmitVoteAsync_DifferentUsers_CanVoteSuccessfully()
        {
            // Arrange
            using var context = new PaperlessMeetingDbContext(_dbOptions);
            await SeedPollDataAsync(context);
            var service = new PollService(context);

            // Act: 2 người dùng khác nhau biểu quyết cùng 1 cuộc thăm dò
            var user1Vote = await service.SubmitVoteAsync(pollId: 1, userId: 2, choice: true);
            var user2Vote = await service.SubmitVoteAsync(pollId: 1, userId: 3, choice: false);

            // Assert: Cả 2 đều hợp lệ
            user1Vote.Should().BeTrue();
            user2Vote.Should().BeTrue();

            var totalVotes = await context.UserVotes.CountAsync(v => v.PollId == 1);
            totalVotes.Should().Be(2);
        }

        [Fact]
        public async Task SubmitVoteAsync_ClosedPoll_ReturnsFalse()
        {
            // Arrange
            using var context = new PaperlessMeetingDbContext(_dbOptions);
            await SeedPollDataAsync(context);
            var service = new PollService(context);

            // Act: Bỏ phiếu vào cuộc thăm dò đã đóng
            var result = await service.SubmitVoteAsync(pollId: 2, userId: 2, choice: true);

            // Assert
            result.Should().BeFalse();
        }
    }
}
