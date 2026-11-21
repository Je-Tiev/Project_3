using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Chat;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class ChatServiceTests
    {
        private PaperlessMeetingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PaperlessMeetingDbContext(options);
        }

        [Fact]
        public async Task SaveMessage_PersistsMessageAndReturnsDto()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "Hành chính" });
            context.Users.Add(new User
            {
                UserId = 3,
                Username = "user3",
                FullName = "Nguyễn Văn C",
                Email = "c@test.vn",
                PasswordHash = "h",
                DepartmentId = 1,
                Role = User.UserRole.User
            });
            await context.SaveChangesAsync();

            var service = new ChatService(context);

            // Act
            var msg = await service.SaveMessageAsync(meetingId: 10, userId: 3, message: "Xin chào mọi người");

            // Assert
            msg.Should().NotBeNull();
            msg.MeetingId.Should().Be(10);
            msg.UserId.Should().Be(3);
            msg.UserFullName.Should().Be("Nguyễn Văn C");
            msg.Message.Should().Be("Xin chào mọi người");

            var history = await service.GetChatHistoryAsync(meetingId: 10);
            history.Should().HaveCount(1);
        }
    }
}
