using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Paperless_Meeting.Data;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Document;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class DocumentSecurityTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv = new();
        private readonly Mock<IConfiguration> _mockConfig = new();
        private readonly Mock<ILogger<DocumentService>> _mockLogger = new();

        private PaperlessMeetingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PaperlessMeetingDbContext(options);
        }

        private async Task SeedMeetingDataAsync(PaperlessMeetingDbContext context)
        {
            // Seed Department
            context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "IT" });

            // Seed Users
            context.Users.AddRange(
                new User { UserId = 1, Username = "host", FullName = "Host User", Email = "host@hust.edu.vn", PasswordHash = "h", DepartmentId = 1, Role = User.UserRole.User },
                new User { UserId = 2, Username = "uploader", FullName = "Uploader User", Email = "uploader@hust.edu.vn", PasswordHash = "h", DepartmentId = 1, Role = User.UserRole.User },
                new User { UserId = 3, Username = "participant", FullName = "Participant User", Email = "participant@hust.edu.vn", PasswordHash = "h", DepartmentId = 1, Role = User.UserRole.User },
                new User { UserId = 4, Username = "outsider", FullName = "Outsider User", Email = "outsider@hust.edu.vn", PasswordHash = "h", DepartmentId = 1, Role = User.UserRole.User },
                new User { UserId = 99, Username = "admin", FullName = "Admin User", Email = "admin@hust.edu.vn", PasswordHash = "h", DepartmentId = 1, Role = User.UserRole.Admin }
            );

            // Seed Meeting (Host = UserId 1)
            var meeting = new Meeting
            {
                MeetingId = 10,
                Title = "Confidential Board Meeting",
                RoomId = "ROOM-10",
                Location = "Hall A",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                CreatedByUserId = 1,
                Status = Meeting.MeetingStatus.Scheduled
            };
            context.Meetings.Add(meeting);

            // Seed Participants: UserId 1 (Host) and UserId 3 (Participant)
            context.MeetingParticipants.AddRange(
                new MeetingParticipant { MeetingId = 10, UserId = 1, RoleInMeeting = MeetingParticipant.MeetingRole.Host, IsPresent = true },
                new MeetingParticipant { MeetingId = 10, UserId = 3, RoleInMeeting = MeetingParticipant.MeetingRole.Participant, IsPresent = true }
            );

            // Seed Document (Uploaded by UserId 2 in Meeting 10)
            context.Documents.Add(new Document
            {
                DocumentId = 100,
                MeetingId = 10,
                DocumentName = "Financial_Report_Q4.pdf",
                FilePath = "uploads/documents/guid_Financial_Report_Q4.pdf",
                UploadedBy = 2,
                UploadedAt = DateTime.UtcNow,
                IsSecret = true
            });

            await context.SaveChangesAsync();
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("SuperAdmin")]
        public async Task CanUserAccessDocumentAsync_AdminOrSuperAdmin_ReturnsTrue(string role)
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedMeetingDataAsync(context);
            var service = new DocumentService(context, _mockEnv.Object, _mockConfig.Object, _mockLogger.Object);

            // Act: Admin accesses document
            var canAccess = await service.CanUserAccessDocumentAsync(documentId: 100, userId: 99, userRole: role);

            // Assert
            canAccess.Should().BeTrue();
        }

        [Fact]
        public async Task CanUserAccessDocumentAsync_DocumentUploader_ReturnsTrue()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedMeetingDataAsync(context);
            var service = new DocumentService(context, _mockEnv.Object, _mockConfig.Object, _mockLogger.Object);

            // Act: Uploader (UserId = 2) accesses document
            var canAccess = await service.CanUserAccessDocumentAsync(documentId: 100, userId: 2, userRole: "User");

            // Assert
            canAccess.Should().BeTrue();
        }

        [Fact]
        public async Task CanUserAccessDocumentAsync_MeetingHost_ReturnsTrue()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedMeetingDataAsync(context);
            var service = new DocumentService(context, _mockEnv.Object, _mockConfig.Object, _mockLogger.Object);

            // Act: Meeting Host (UserId = 1) accesses document
            var canAccess = await service.CanUserAccessDocumentAsync(documentId: 100, userId: 1, userRole: "User");

            // Assert
            canAccess.Should().BeTrue();
        }

        [Fact]
        public async Task CanUserAccessDocumentAsync_MeetingParticipant_ReturnsTrue()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedMeetingDataAsync(context);
            var service = new DocumentService(context, _mockEnv.Object, _mockConfig.Object, _mockLogger.Object);

            // Act: Participant (UserId = 3) accesses document
            var canAccess = await service.CanUserAccessDocumentAsync(documentId: 100, userId: 3, userRole: "User");

            // Assert
            canAccess.Should().BeTrue();
        }

        [Fact]
        public async Task CanUserAccessDocumentAsync_OutsiderUser_ReturnsFalse_PreventingIDOR()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedMeetingDataAsync(context);
            var service = new DocumentService(context, _mockEnv.Object, _mockConfig.Object, _mockLogger.Object);

            // Act: Outsider (UserId = 4) tries to access confidential document of Meeting 10
            var canAccess = await service.CanUserAccessDocumentAsync(documentId: 100, userId: 4, userRole: "User");

            // Assert: Must be rejected (Anti-IDOR)
            canAccess.Should().BeFalse();
        }

        [Fact]
        public async Task CanUserAccessDocumentAsync_NonExistentDocument_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            await SeedMeetingDataAsync(context);
            var service = new DocumentService(context, _mockEnv.Object, _mockConfig.Object, _mockLogger.Object);

            // Act
            var canAccess = await service.CanUserAccessDocumentAsync(documentId: 99999, userId: 1, userRole: "User");

            // Assert
            canAccess.Should().BeFalse();
        }
    }
}
