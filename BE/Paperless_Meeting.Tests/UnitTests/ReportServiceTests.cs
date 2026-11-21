using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Report;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class ReportServiceTests
    {
        private PaperlessMeetingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PaperlessMeetingDbContext(options);
        }

        [Fact]
        public async Task GetOverviewReport_ReturnsRealMetrics()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "Kế toán" });
            context.Users.Add(new User
            {
                UserId = 1,
                Username = "admin",
                FullName = "Quản trị viên",
                Email = "admin@test.vn",
                PasswordHash = "h",
                DepartmentId = 1,
                Role = User.UserRole.Admin
            });

            context.Rooms.Add(new Room
            {
                RoomId = 1,
                RoomName = "Phòng Hội nghị 1",
                Capacity = 20,
                CreatedAt = DateTime.UtcNow
            });

            context.Meetings.Add(new Meeting
            {
                MeetingId = 1,
                Title = "Họp Giao Ban",
                RoomId = "MEET-1",
                CreatedByUserId = 1,
                PhysicalRoomId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var service = new ReportService(context);

            // Act
            var report = await service.GetOverviewReportAsync();

            // Assert
            report.Should().NotBeNull();
            report.TotalMeetings.Should().Be(1);
            report.TotalUsers.Should().Be(1);
            report.TotalRooms.Should().Be(1);
            report.RoomUtilization.Should().HaveCount(1);
            report.RoomUtilization[0].MeetingCount.Should().Be(1);
            report.MonthlyMeetings.Should().NotBeEmpty();
        }
    }
}
