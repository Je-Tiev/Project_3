using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Conclusion;

public class ConclusionUpsertDto
{
    [Required]
    public string Summary { get; set; } = string.Empty;

    public string? Status { get; set; } = "Draft";
}
