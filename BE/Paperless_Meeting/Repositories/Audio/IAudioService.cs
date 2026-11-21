using Microsoft.AspNetCore.Http;
using Paperless_Meeting.DTOs.Audio;

namespace Paperless_Meeting.Repositories.Audio;

public interface IAudioService
{
    Task<AudioDto> SaveAudioRecordingAsync(int meetingId, int userId, IFormFile file, float duration = 0);
    Task<List<AudioDto>> GetAudiosByMeetingIdAsync(int meetingId);
    Task<(string filePath, string contentType)?> GetAudioFilePathAsync(int audioId, int requesterUserId);
}
