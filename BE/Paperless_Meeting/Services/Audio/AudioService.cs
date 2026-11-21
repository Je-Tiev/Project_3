using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Audio;
using Paperless_Meeting.Models;
using Paperless_Meeting.Repositories.Audio;

namespace Paperless_Meeting.Services.Audio;

public class AudioService : IAudioService
{
    private readonly PaperlessMeetingDbContext _context;
    private readonly string _uploadPath;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".webm", ".mp3", ".wav", ".ogg", ".m4a"
    };

    public AudioService(PaperlessMeetingDbContext context, IConfiguration config)
    {
        _context = context;
        _uploadPath = config["Storage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
    }

    public async Task<AudioDto> SaveAudioRecordingAsync(int meetingId, int userId, IFormFile file, float duration = 0)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Tệp âm thanh rỗng.");

        if (file.Length > 200 * 1024 * 1024) // 200MB max
            throw new ArgumentException("Dung lượng tệp âm thanh vượt quá giới hạn 200MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new ArgumentException("Định dạng tệp âm thanh không được hỗ trợ (chỉ nhận .webm, .mp3, .wav, .ogg, .m4a).");

        // Validate Magic Bytes
        using (var stream = file.OpenReadStream())
        {
            var header = new byte[8];
            int read = await stream.ReadAsync(header, 0, header.Length);
            if (!IsValidAudioHeader(header, ext))
            {
                throw new ArgumentException("Nội dung tệp âm thanh không hợp lệ hoặc đã bị chỉnh sửa.");
            }
        }

        var audioDir = Path.Combine(_uploadPath, "audios");
        if (!Directory.Exists(audioDir))
        {
            Directory.CreateDirectory(audioDir);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(audioDir, uniqueFileName);

        using (var destStream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(destStream);
        }

        var relativePath = Path.Combine("audios", uniqueFileName).Replace("\\", "/");
        var audio = new MeetingAudio
        {
            MeetingId = meetingId,
            UserId = userId,
            FilePath = relativePath,
            Duration = duration,
            CreatedAt = DateTime.UtcNow
        };

        _context.MeetingAudios.Add(audio);
        await _context.SaveChangesAsync();

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);

        return new AudioDto
        {
            AudioId = audio.AudioId,
            MeetingId = audio.MeetingId,
            UserId = audio.UserId,
            SpeakerName = user?.FullName ?? $"User {userId}",
            FilePath = audio.FilePath,
            Duration = audio.Duration,
            TranscribedText = audio.TranscribedText,
            CreatedAt = audio.CreatedAt
        };
    }

    public async Task<List<AudioDto>> GetAudiosByMeetingIdAsync(int meetingId)
    {
        return await _context.MeetingAudios
            .Where(a => a.MeetingId == meetingId)
            .Include(a => a.Speaker)
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .Select(a => new AudioDto
            {
                AudioId = a.AudioId,
                MeetingId = a.MeetingId,
                UserId = a.UserId,
                SpeakerName = a.Speaker != null ? a.Speaker.FullName : $"User {a.UserId}",
                FilePath = a.FilePath,
                Duration = a.Duration,
                TranscribedText = a.TranscribedText,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<(string filePath, string contentType)?> GetAudioFilePathAsync(int audioId, int requesterUserId)
    {
        var audio = await _context.MeetingAudios
            .Include(a => a.Meeting)
                .ThenInclude(m => m.Participants)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AudioId == audioId);

        if (audio == null) return null;

        var meeting = audio.Meeting;
        var hasAccess = meeting.CreatedByUserId == requesterUserId || meeting.Participants.Any(p => p.UserId == requesterUserId);
        if (!hasAccess) return null;

        var fullPath = Path.Combine(_uploadPath, audio.FilePath);
        if (!File.Exists(fullPath)) return null;

        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".webm" => "audio/webm",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            _ => "application/octet-stream"
        };

        return (fullPath, contentType);
    }

    private static bool IsValidAudioHeader(byte[] header, string ext)
    {
        if (header.Length < 4) return false;

        return ext switch
        {
            // WebM / Matroska: 1A 45 DF A3
            ".webm" => header[0] == 0x1A && header[1] == 0x45 && header[2] == 0xDF && header[3] == 0xA3,
            // MP3: ID3 or FF FB / FF F3
            ".mp3" => (header[0] == 0x49 && header[1] == 0x44 && header[2] == 0x33) ||
                      (header[0] == 0xFF && (header[1] & 0xFE) == 0xFA),
            // RIFF WAV: 52 49 46 46
            ".wav" => header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x47 ||
                      (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46),
            // Ogg: 4F 67 67 53
            ".ogg" => header[0] == 0x4F && header[1] == 0x67 && header[2] == 0x67 && header[3] == 0x53,
            // M4A / MP4: ....ftyp
            ".m4a" => header.Length >= 8 && header[4] == 0x66 && header[5] == 0x74 && header[6] == 0x79 && header[7] == 0x70,
            _ => false
        };
    }
}
