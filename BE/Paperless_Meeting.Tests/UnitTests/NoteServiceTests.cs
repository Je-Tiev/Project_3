using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Note;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class NoteServiceTests
    {
        private PaperlessMeetingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PaperlessMeetingDbContext(options);
        }

        [Fact]
        public async Task UpsertNote_WhenNoteDoesNotExist_CreatesNewNote()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var service = new NoteService(context);

            // Act
            var note = await service.UpsertNoteAsync(meetingId: 5, userId: 10, content: "Ý kiến ban đầu về ngân sách");

            // Assert
            note.Should().NotBeNull();
            note.MeetingId.Should().Be(5);
            note.UserId.Should().Be(10);
            note.Content.Should().Be("Ý kiến ban đầu về ngân sách");
            note.UpdatedAt.Should().BeNull();
        }

        [Fact]
        public async Task UpsertNote_WhenNoteAlreadyExists_UpdatesContentAndSetsUpdatedAt()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var service = new NoteService(context);

            await service.UpsertNoteAsync(meetingId: 5, userId: 10, content: "Bản thảo 1");

            // Act - update note
            var updated = await service.UpsertNoteAsync(meetingId: 5, userId: 10, content: "Bản thảo 2 có chỉnh sửa");

            // Assert
            updated.Content.Should().Be("Bản thảo 2 có chỉnh sửa");
            updated.UpdatedAt.Should().NotBeNull();

            // Verify in DB only 1 note exists
            var totalNotes = await context.Notes.CountAsync(n => n.MeetingId == 5 && n.UserId == 10);
            totalNotes.Should().Be(1);
        }
    }
}
