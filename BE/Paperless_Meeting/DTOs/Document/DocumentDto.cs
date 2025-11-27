using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Document;

public class DocumentDto
{
    public int DocumentId { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; }

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; }

    [Required]
    public string Visibility { get; set; } // "Chung" hoặc "NoiBo"

    public int UploadedBy { get; set; }

    public DateTime UploadDate { get; set; }
}