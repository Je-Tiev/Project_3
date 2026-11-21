using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Note;
using Paperless_Meeting.Models;
using Paperless_Meeting.Repositories.Note;

namespace Paperless_Meeting.Services.Note;

public class NoteService : INoteService
{
    private readonly PaperlessMeetingDbContext _context;

    public NoteService(PaperlessMeetingDbContext context)
    {
        _context = context;
    }

    public async Task<NoteDto?> GetNoteByMeetingAndUserAsync(int meetingId, int userId)
    {
        var note = await _context.Notes
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MeetingId == meetingId && n.UserId == userId);

        if (note == null) return null;

        return new NoteDto
        {
            NoteId = note.NoteId,
            MeetingId = note.MeetingId,
            UserId = note.UserId,
            Content = note.Content,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }

    public async Task<NoteDto> UpsertNoteAsync(int meetingId, int userId, string content)
    {
        var existingNote = await _context.Notes
            .FirstOrDefaultAsync(n => n.MeetingId == meetingId && n.UserId == userId);

        if (existingNote != null)
        {
            existingNote.Content = content ?? string.Empty;
            existingNote.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new NoteDto
            {
                NoteId = existingNote.NoteId,
                MeetingId = existingNote.MeetingId,
                UserId = existingNote.UserId,
                Content = existingNote.Content,
                CreatedAt = existingNote.CreatedAt,
                UpdatedAt = existingNote.UpdatedAt
            };
        }

        var newNote = new Models.Note
        {
            MeetingId = meetingId,
            UserId = userId,
            Content = content ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _context.Notes.Add(newNote);
        await _context.SaveChangesAsync();

        return new NoteDto
        {
            NoteId = newNote.NoteId,
            MeetingId = newNote.MeetingId,
            UserId = newNote.UserId,
            Content = newNote.Content,
            CreatedAt = newNote.CreatedAt,
            UpdatedAt = newNote.UpdatedAt
        };
    }
}
