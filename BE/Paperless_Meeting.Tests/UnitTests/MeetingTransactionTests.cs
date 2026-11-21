using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Meeting;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class MeetingTransactionTests : IDisposable
    {
        private readonly SqliteConnection _sqliteConnection;
        private readonly DbContextOptions<PaperlessMeetingDbContext> _dbOptions;

        public MeetingTransactionTests()
        {
            // SQLite In-Memory hỗ trợ transaction nguyên khối và Foreign Key constraints
            _sqliteConnection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
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

        private async Task SeedBaseDataAsync(PaperlessMeetingDbContext context)
        {
            context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "Kế toán" });
            context.Users.Add(new User
            {
                UserId = 10,
                Username = "validhost",
                FullName = "Valid Host",
                Email = "host@hust.edu.vn",
                PasswordHash = "hashed",
                DepartmentId = 1,
                Role = User.UserRole.User
            });
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateMeetingAsync_ValidData_CreatesMeetingAndHostParticipantAtomically()
        {
            // Arrange
            using var context = new PaperlessMeetingDbContext(_dbOptions);
            await SeedBaseDataAsync(context);
            var service = new MeetingService(context);

            var createDto = new MeetingCreateDto
            {
                Title = "Họp Giao Ban Đầu Năm",
                RoomId = "ROOM-GB01",
                Description = "Kế hoạch quý 1",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Phòng họp số 3"
            };

            // Act
            var result = await service.CreateMeetingAsync(createDto, creatorId: 10);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Họp Giao Ban Đầu Năm");

            // Kiểm tra Meeting được tạo trong database
            var savedMeeting = await context.Meetings.FirstOrDefaultAsync(m => m.RoomId == "ROOM-GB01");
            savedMeeting.Should().NotBeNull();
            savedMeeting!.CreatedByUserId.Should().Be(10);

            // Kiểm tra Host Participant được tạo đồng thời
            var savedParticipant = await context.MeetingParticipants
                .FirstOrDefaultAsync(p => p.MeetingId == savedMeeting.MeetingId && p.UserId == 10);
            savedParticipant.Should().NotBeNull();
            savedParticipant!.RoleInMeeting.Should().Be(MeetingParticipant.MeetingRole.Host);
        }

        [Fact]
        public async Task CreateMeetingAsync_InvalidCreator_RollsBackTransaction_LeavesNoOrphanMeeting()
        {
            // Arrange
            using var context = new PaperlessMeetingDbContext(_dbOptions);
            await SeedBaseDataAsync(context);
            var service = new MeetingService(context);

            var createDto = new MeetingCreateDto
            {
                Title = "Họp với User Không Tồn Tại",
                RoomId = "ROOM-FAIL",
                Description = "Sẽ rollback",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Phòng ảo"
            };

            // Act: Cố tình tạo meeting với CreatorId không tồn tại (UserId 99999)
            // Lệnh sẽ ném DbUpdateException do vi phạm Foreign Key constraint của Users
            Func<Task> act = async () => await service.CreateMeetingAsync(createDto, creatorId: 99999);

            // Assert
            await act.Should().ThrowAsync<DbUpdateException>();

            // Do bọc trong Transaction, không có meeting nào bị lưu lại (Zero Orphan Records)
            var orphanMeeting = await context.Meetings.FirstOrDefaultAsync(m => m.RoomId == "ROOM-FAIL");
            orphanMeeting.Should().BeNull();
        }
    }
}
