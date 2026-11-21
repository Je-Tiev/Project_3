using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Paperless_Meeting.Data;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Ai;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class AiSummaryServiceTests
    {
        private PaperlessMeetingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PaperlessMeetingDbContext(options);
        }

        [Fact]
        public async Task GenerateMeetingSummary_ProducesStructuredMinutes()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "Kỹ thuật" });
            context.Users.Add(new User
            {
                UserId = 1,
                Username = "chairman",
                FullName = "Chủ tọa Đặng Văn A",
                Email = "chairman@test.vn",
                PasswordHash = "h",
                DepartmentId = 1,
                Role = User.UserRole.Admin
            });

            var meeting = new Meeting
            {
                MeetingId = 10,
                Title = "Hội nghị Triển khai Kế hoạch 2026",
                Description = "Thảo luận ngân sách và chuyển đổi số",
                RoomId = "MEET-2026",
                CreatedByUserId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2),
                CreatedAt = DateTime.UtcNow
            };
            context.Meetings.Add(meeting);

            // Add Poll with votes
            var poll = new Poll
            {
                PollId = 1,
                MeetingId = 10,
                VoteByUserId = 1,
                Question = "Thông qua kế hoạch chuyển đổi số?",
                Status = Poll.PollStatus.Closed,
                CreatedAt = DateTime.UtcNow
            };
            context.Polls.Add(poll);
            context.UserVotes.Add(new UserVote
            {
                VoteId = 1,
                PollId = 1,
                UserId = 1,
                Choice = true,
                VotedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var config = new ConfigurationBuilder().Build();
            var service = new AiSummaryService(context, config);

            // Act
            var summary = await service.GenerateMeetingSummaryAsync(10);

            // Assert
            summary.Should().NotBeNullOrWhiteSpace();
            summary.Should().Contain("Hội nghị Triển khai Kế hoạch 2026");
            summary.Should().Contain("Chủ tọa Đặng Văn A");
            summary.Should().Contain("Thông qua kế hoạch chuyển đổi số?");
            summary.Should().Contain("ĐÃ THÔNG QUA");
            summary.Should().Contain("PHÂN CÔNG NHIỆM VỤ");
        }
    }
}
