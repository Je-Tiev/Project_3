using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.MeetingLog;
using Paperless_Meeting.Models;
using Paperless_Meeting.Repositories.MeetingLog;

namespace Paperless_Meeting.Services.MeetingLog;

public class MeetingLogService : IMeetingLogService
{
    private readonly PaperlessMeetingDbContext _context;

    public MeetingLogService(PaperlessMeetingDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(int meetingId, int userId, string action)
    {
        var log = new Models.MeetingLog
        {
            MeetingId = meetingId,
            UserId = userId,
            Action = action.Length > 100 ? action.Substring(0, 100) : action,
            ActionTime = DateTime.UtcNow
        };

        _context.MeetingLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MeetingLogDto>> GetLogsByMeetingIdAsync(int meetingId)
    {
        return await _context.MeetingLogs
            .Where(l => l.MeetingId == meetingId)
            .Include(l => l.UserAction)
            .OrderByDescending(l => l.ActionTime)
            .AsNoTracking()
            .Select(l => new MeetingLogDto
            {
                LogId = l.LogId,
                MeetingId = l.MeetingId,
                UserId = l.UserId,
                UserName = l.UserAction != null ? l.UserAction.FullName : $"User {l.UserId}",
                Action = l.Action,
                ActionTime = l.ActionTime
            })
            .ToListAsync();
    }
}
