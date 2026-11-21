namespace Paperless_Meeting.DTOs.Conclusion;

public class ConclusionSignatureDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
}
