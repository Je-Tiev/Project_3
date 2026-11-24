using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Conclusion;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Conclusion;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class ConclusionServiceTests
    {
        private PaperlessMeetingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PaperlessMeetingDbContext(options);
        }

        [Fact]
        public async Task UpsertConclusion_CreatesDraftAndSaves()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var service = new ConclusionService(context);

            // Act
            var result = await service.UpsertConclusionAsync(meetingId: 2, userId: 1, new ConclusionUpsertDto
            {
                Summary = "Đồng ý thông qua báo cáo quý 3.",
                Status = "Draft"
            });

            // Assert
            result.Should().NotBeNull();
            result.Summary.Should().Be("Đồng ý thông qua báo cáo quý 3.");
            result.Status.Should().Be("Draft");
            result.Signatures.Should().BeEmpty();
        }

        [Fact]
        public async Task SignConclusion_AddsSignatureAndPreventsDuplicateSign()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.Users.Add(new User
            {
                UserId = 1,
                Username = "chairman",
                FullName = "Chủ tịch Hội đồng",
                Email = "chairman@hust.edu.vn",
                PasswordHash = "hashed",
                DepartmentId = 1,
                Role = User.UserRole.User
            });
            await context.SaveChangesAsync();

            var service = new ConclusionService(context);

            await service.UpsertConclusionAsync(meetingId: 2, userId: 1, new ConclusionUpsertDto
            {
                Summary = "Đồng ý phương án nhân sự.",
                Status = "Draft"
            });

            // Act - user 1 signs
            var signed1 = await service.SignConclusionAsync(meetingId: 2, userId: 1, userName: "Chủ tịch Hội đồng");
            // Act - user 1 tries to sign again
            var signedAgain = await service.SignConclusionAsync(meetingId: 2, userId: 1, userName: "Chủ tịch Hội đồng");

            // Assert
            signed1.Should().NotBeNull();
            signed1!.Signatures.Should().HaveCount(1);
            signed1.Signatures[0].FullName.Should().Be("Chủ tịch Hội đồng");

            signedAgain!.Signatures.Should().HaveCount(1); // No duplicate
        }
    }
}
