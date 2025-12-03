using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Document;

public class DocumentMultipleUploadDto
{
    [Required]
    [MinLength(1)]
    public List<IFormFile> Files { get; set; }

    [Required]
    [RegularExpression("^(Chung|NoiBo)$")]
    public string Visibility { get; set; }
}