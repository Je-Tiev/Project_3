using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Poll;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Poll;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class PollSecretBallotTests
    {
        private PaperlessMeetingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PaperlessMeetingDbContext(options);
        }

        private async Task SeedDataAsync(PaperlessMeetingDbContext context)
        {
            context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "Kỹ thuật" });
            context.Users.AddRange(
                new User
                {
                    UserId = 1,
                    Username = "host",
                    FullName = "Chủ tọa Họp",
                    Email = "host@test.vn",
                    PasswordHash = "hash",
                    DepartmentId = 1,
                    Role = User.UserRole.Admin
                },
                new User
                {
                    UserId = 2,
                    Username = "delegate1",
                    FullName = "Đại biểu Nguyễn Văn A",
                    Email = "userA@test.vn",
                    PasswordHash = "hash",
                    DepartmentId = 1,
                    Role = User.UserRole.User
                }
            );

            context.Meetings.Add(new Meeting
            {
                MeetingId = 10,
                Title = "Họp Đại hội Cổ đông",
                RoomId = "MEET-SECRET",
                CreatedByUserId = 1,
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Status = Meeting.MeetingStatus.Ongoing,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreatePoll_WithIsSecretTrue_MasksVoterIdentities()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedDataAsync(context);
            var pollService = new PollService(context);

            // Act - Create secret poll
            var created = await pollService.CreatePollAsync(new PollCreateRequestDto
            {
                MeetingId = 10,
                Question = "Bỏ phiếu kín miễn nhiệm?",
                IsSecret = true
            }, creatorId: 1);

            // Delegate votes
            await pollService.SubmitVoteAsync(created.PollId, userId: 2, choice: true);

            // Fetch poll detail
            var pollDetail = await pollService.GetPollByIdAsync(created.PollId, currentUserId: 1);

            // Assert
            pollDetail.Should().NotBeNull();
            pollDetail!.IsSecret.Should().BeTrue();
            pollDetail.TotalVotes.Should().Be(1);
            pollDetail.YesVotes.Should().Be(1);
            pollDetail.Votes.Should().HaveCount(1);
            pollDetail.Votes[0].UserId.Should().Be(0); // Masked
            pollDetail.Votes[0].FullName.Should().Contain("Bỏ phiếu kín");
        }

        [Fact]
        public async Task CreatePoll_WithIsSecretFalse_ShowsVoterIdentities()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedDataAsync(context);
            var pollService = new PollService(context);

            // Act - Create open poll
            var created = await pollService.CreatePollAsync(new PollCreateRequestDto
            {
                MeetingId = 10,
                Question = "Biểu quyết công khai thông qua kế hoạch?",
                IsSecret = false
            }, creatorId: 1);

            await pollService.SubmitVoteAsync(created.PollId, userId: 2, choice: true);

            var pollDetail = await pollService.GetPollByIdAsync(created.PollId, currentUserId: 1);

            // Assert
            pollDetail.Should().NotBeNull();
            pollDetail!.IsSecret.Should().BeFalse();
            pollDetail.TotalVotes.Should().Be(1);
            pollDetail.Votes[0].UserId.Should().Be(2);
            pollDetail.Votes[0].FullName.Should().Be("Đại biểu Nguyễn Văn A");
        }
    }
}
