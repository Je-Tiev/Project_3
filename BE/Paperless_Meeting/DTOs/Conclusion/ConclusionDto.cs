namespace Paperless_Meeting.DTOs.Conclusion;

public class ConclusionDto
{
    public int ConclusionId { get; set; }
    public int MeetingId { get; set; }
    public int ConclusionByUserId { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // "Draft", "Finalized"
    public List<ConclusionSignatureDto> Signatures { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
