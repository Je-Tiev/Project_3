using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Document;

public class DocumentUploadDto
{
    [Required]
    public IFormFile File { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; }

    [Required]
    public string Visibility { get; set; } // "Chung" hoặc "NoiBo"
}
