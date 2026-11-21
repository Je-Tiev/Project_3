using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Chat;
using Paperless_Meeting.Models;
using Paperless_Meeting.Repositories.Chat;

namespace Paperless_Meeting.Services.Chat;

public class ChatService : IChatService
{
    private readonly PaperlessMeetingDbContext _context;

    public ChatService(PaperlessMeetingDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessageDto> SaveMessageAsync(int meetingId, int userId, string message)
    {
        var chat = new MeetingChat
        {
            MeetingId = meetingId,
            UserId = userId,
            Message = message,
            SentAt = DateTime.UtcNow
        };

        _context.MeetingChats.Add(chat);
        await _context.SaveChangesAsync();

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);

        return new ChatMessageDto
        {
            ChatId = chat.ChatId,
            MeetingId = chat.MeetingId,
            UserId = chat.UserId,
            UserFullName = user?.FullName ?? $"User {userId}",
            Message = chat.Message,
            SentAt = chat.SentAt
        };
    }

    public async Task<List<ChatMessageDto>> GetChatHistoryAsync(int meetingId)
    {
        return await _context.MeetingChats
            .Where(c => c.MeetingId == meetingId)
            .Include(c => c.User)
            .OrderBy(c => c.SentAt)
            .AsNoTracking()
            .Select(c => new ChatMessageDto
            {
                ChatId = c.ChatId,
                MeetingId = c.MeetingId,
                UserId = c.UserId,
                UserFullName = c.User != null ? c.User.FullName : $"User {c.UserId}",
                Message = c.Message,
                SentAt = c.SentAt
            })
            .ToListAsync();
    }
}
