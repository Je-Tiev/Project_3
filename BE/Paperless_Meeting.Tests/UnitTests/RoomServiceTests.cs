using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Room;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Room;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class RoomServiceTests
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
            context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "Ban Giám đốc" });
            context.Users.Add(new User
            {
                UserId = 1,
                Username = "admin",
                FullName = "Admin User",
                Email = "admin@hust.edu.vn",
                PasswordHash = "h",
                DepartmentId = 1,
                Role = User.UserRole.Admin
            });

            context.Rooms.AddRange(
                new Room
                {
                    RoomId = 1,
                    RoomName = "Conference Room A",
                    Location = "Tầng 2",
                    Capacity = 30,
                    Equipment = "LED, Camera 360",
                    Status = RoomStatus.Available,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    RoomId = 2,
                    RoomName = "Maintenance Room",
                    Location = "Tầng 1",
                    Capacity = 10,
                    Equipment = "None",
                    Status = RoomStatus.Maintenance,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed Meeting using Room 1 tomorrow 10:00 to 12:00
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            context.Meetings.Add(new Meeting
            {
                MeetingId = 10,
                PhysicalRoomId = 1,
                Title = "Họp Chiến Lược Ngày Mai",
                RoomId = "ROOM-10",
                Location = "Tầng 2",
                StartTime = tomorrow.AddHours(10),
                EndTime = tomorrow.AddHours(12),
                CreatedByUserId = 1,
                Status = Meeting.MeetingStatus.Scheduled
            });

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllRoomsAsync_ReturnsAllRooms()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new RoomService(context);

            // Act
            var rooms = await service.GetAllRoomsAsync();

            // Assert
            rooms.Should().HaveCount(2);
            rooms.Should().Contain(r => r.RoomName == "Conference Room A");
            rooms.Should().Contain(r => r.RoomName == "Maintenance Room" && r.Status == "Maintenance");
        }

        [Fact]
        public async Task CreateRoomAsync_ValidData_AddsRoomSuccessfully()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var service = new RoomService(context);

            var createDto = new RoomCreateDto
            {
                RoomName = "Phòng Hội Thảo B1",
                Location = "Tòa nhà B - Tầng 1",
                Capacity = 100,
                Equipment = "Máy chiếu laser, Âm thanh vòm",
                Status = "Available"
            };

            // Act
            var created = await service.CreateRoomAsync(createDto);

            // Assert
            created.Should().NotBeNull();
            created.RoomName.Should().Be("Phòng Hội Thảo B1");
            created.Capacity.Should().Be(100);

            var saved = await context.Rooms.FirstOrDefaultAsync(r => r.RoomName == "Phòng Hội Thảo B1");
            saved.Should().NotBeNull();
        }

        [Fact]
        public async Task IsRoomAvailableAsync_NoOverlappingMeetings_ReturnsTrue()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new RoomService(context);

            var tomorrow = DateTime.UtcNow.Date.AddDays(1);

            // Act: Khung giờ 14:00 - 16:00 ngày mai (không trùng với 10:00 - 12:00)
            var isAvailable = await service.IsRoomAvailableAsync(
                roomId: 1,
                startTime: tomorrow.AddHours(14),
                endTime: tomorrow.AddHours(16)
            );

            // Assert
            isAvailable.Should().BeTrue();
        }

        [Fact]
        public async Task IsRoomAvailableAsync_OverlappingMeeting_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new RoomService(context);

            var tomorrow = DateTime.UtcNow.Date.AddDays(1);

            // Act: Khung giờ 11:00 - 13:00 ngày mai (bị trùng với cuộc họp 10:00 - 12:00)
            var isAvailable = await service.IsRoomAvailableAsync(
                roomId: 1,
                startTime: tomorrow.AddHours(11),
                endTime: tomorrow.AddHours(13)
            );

            // Assert
            isAvailable.Should().BeFalse();
        }

        [Fact]
        public async Task IsRoomAvailableAsync_RoomInMaintenance_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new RoomService(context);

            var tomorrow = DateTime.UtcNow.Date.AddDays(1);

            // Act: Phòng đang bảo trì (RoomId = 2)
            var isAvailable = await service.IsRoomAvailableAsync(
                roomId: 2,
                startTime: tomorrow.AddHours(8),
                endTime: tomorrow.AddHours(10)
            );

            // Assert
            isAvailable.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteRoomAsync_RoomWithScheduledMeeting_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new RoomService(context);

            // Act: Cố gắng xóa Room 1 đang có cuộc họp Scheduled
            var success = await service.DeleteRoomAsync(1);

            // Assert
            success.Should().BeFalse();
            var room = await context.Rooms.FindAsync(1);
            room.Should().NotBeNull();
        }
    }
}
