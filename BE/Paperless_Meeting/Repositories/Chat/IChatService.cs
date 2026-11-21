using Paperless_Meeting.DTOs.Chat;

namespace Paperless_Meeting.Repositories.Chat;

public interface IChatService
{
    Task<ChatMessageDto> SaveMessageAsync(int meetingId, int userId, string message);
    Task<List<ChatMessageDto>> GetChatHistoryAsync(int meetingId);
}
