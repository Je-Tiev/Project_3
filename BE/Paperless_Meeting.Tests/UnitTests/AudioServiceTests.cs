using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Paperless_Meeting.Data;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Audio;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class AudioServiceTests
    {
        private PaperlessMeetingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PaperlessMeetingDbContext(options);
        }

        private IConfiguration CreateMockConfiguration(string tempDir)
        {
            var inMemorySettings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Storage:UploadPath", tempDir }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task SaveAudioRecording_WithValidWebMHeader_SavesFileAndRecordsDatabase()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), "paperless_test_audio_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            try
            {
                using var context = CreateInMemoryDbContext();
                context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "Hành chính" });
                context.Users.Add(new User
                {
                    UserId = 1,
                    Username = "host",
                    FullName = "Chủ tọa",
                    Email = "host@test.vn",
                    PasswordHash = "h",
                    DepartmentId = 1,
                    Role = User.UserRole.Admin
                });
                await context.SaveChangesAsync();

                var config = CreateMockConfiguration(tempDir);
                var service = new AudioService(context, config);

                // Mock WebM file with valid 1A 45 DF A3 magic bytes
                var webmBytes = new byte[] { 0x1A, 0x45, 0xDF, 0xA3, 0x01, 0x00, 0x00, 0x00, 0xAA, 0xBB };
                var formFile = new FormFile(new MemoryStream(webmBytes), 0, webmBytes.Length, "file", "recording.webm");

                // Act
                var result = await service.SaveAudioRecordingAsync(meetingId: 5, userId: 1, formFile, duration: 120);

                // Assert
                result.Should().NotBeNull();
                result.MeetingId.Should().Be(5);
                result.UserId.Should().Be(1);
                result.Duration.Should().Be(120);
                result.FilePath.Should().StartWith("audios");

                var audiosInDb = await context.MeetingAudios.CountAsync(a => a.MeetingId == 5);
                audiosInDb.Should().Be(1);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public async Task SaveAudioRecording_WithDisallowedExtension_ThrowsArgumentException()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), "paperless_test_audio_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            try
            {
                using var context = CreateInMemoryDbContext();
                var config = CreateMockConfiguration(tempDir);
                var service = new AudioService(context, config);

                var bytes = Encoding.UTF8.GetBytes("not an audio file");
                var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "malicious.exe");

                // Act & Assert
                var act = async () => await service.SaveAudioRecordingAsync(meetingId: 5, userId: 1, formFile);
                await act.Should().ThrowAsync<ArgumentException>()
                    .WithMessage("*Định dạng tệp âm thanh không được hỗ trợ*");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
