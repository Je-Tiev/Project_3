using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Conclusion;
using Paperless_Meeting.Models;
using Paperless_Meeting.Repositories.Conclusion;

namespace Paperless_Meeting.Services.Conclusion;

public class ConclusionService : IConclusionService
{
    private readonly PaperlessMeetingDbContext _context;

    public ConclusionService(PaperlessMeetingDbContext context)
    {
        _context = context;
    }

    public async Task<ConclusionDto?> GetConclusionByMeetingIdAsync(int meetingId)
    {
        var conclusion = await _context.Conclusions
            .Include(c => c.Creator)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MeetingId == meetingId);

        if (conclusion == null) return null;

        return MapToDto(conclusion);
    }

    public async Task<ConclusionDto> UpsertConclusionAsync(int meetingId, int userId, ConclusionUpsertDto dto)
    {
        var conclusion = await _context.Conclusions
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.MeetingId == meetingId);

        if (conclusion != null)
        {
            conclusion.Summary = dto.Summary;
            if (!string.IsNullOrEmpty(dto.Status))
            {
                conclusion.Status = dto.Status;
            }
            conclusion.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return MapToDto(conclusion);
        }

        var newConclusion = new Models.Conclusion
        {
            MeetingId = meetingId,
            ConclusionByUserId = userId,
            Summary = dto.Summary,
            Status = !string.IsNullOrEmpty(dto.Status) ? dto.Status : "Draft",
            SignedByUsers = "[]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _context.Conclusions.Add(newConclusion);
        await _context.SaveChangesAsync();

        // Load creator
        await _context.Entry(newConclusion).Reference(c => c.Creator).LoadAsync();

        return MapToDto(newConclusion);
    }

    public async Task<ConclusionDto?> SignConclusionAsync(int meetingId, int userId, string userName)
    {
        var conclusion = await _context.Conclusions
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.MeetingId == meetingId);

        if (conclusion == null) return null;

        var signatures = ParseSignatures(conclusion.SignedByUsers);

        // Kiểm tra xem user này đã ký chưa
        if (!signatures.Any(s => s.UserId == userId))
        {
            signatures.Add(new ConclusionSignatureDto
            {
                UserId = userId,
                FullName = userName,
                SignedAt = DateTime.UtcNow
            });

            conclusion.SignedByUsers = JsonSerializer.Serialize(signatures);
            conclusion.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return MapToDto(conclusion);
    }

    private static ConclusionDto MapToDto(Models.Conclusion c)
    {
        return new ConclusionDto
        {
            ConclusionId = c.ConclusionId,
            MeetingId = c.MeetingId,
            ConclusionByUserId = c.ConclusionByUserId,
            CreatorName = c.Creator?.FullName ?? $"User {c.ConclusionByUserId}",
            Summary = c.Summary,
            Status = c.Status,
            Signatures = ParseSignatures(c.SignedByUsers),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }

    private static List<ConclusionSignatureDto> ParseSignatures(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<ConclusionSignatureDto>();
        try
        {
            return JsonSerializer.Deserialize<List<ConclusionSignatureDto>>(json) ?? new List<ConclusionSignatureDto>();
        }
        catch
        {
            return new List<ConclusionSignatureDto>();
        }
    }
}
