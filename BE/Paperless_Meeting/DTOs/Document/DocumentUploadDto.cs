using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Document;

public class DocumentUploadDto
{
    [Required]
    public IFormFile File { get; set; }

    [Required]
    [RegularExpression("^(Chung|NoiBo)$")]
    public string Visibility { get; set; }
}