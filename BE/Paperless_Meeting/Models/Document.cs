using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class Document
{
    [Key]
    public int DocumentId { get; set; }
    
    [Required]
    public int MeetingId { get; set; }
    [ForeignKey("MeetingId")]
    public virtual Meeting Meeting { get; set; }
    
    [Required]
    [StringLength(255)]
    public string FileName { get; set; }
        
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; }

    public enum DocumentVisibility { Chung, NoiBo }
    [Required]
    public DocumentVisibility Visibility { get; set; }
    
    [Required]
    public int UploadedBy { get; set; }
    [ForeignKey("UploadedBy")]
    public virtual User Uploader { get; set; }
    
    [Required]
    public DateTime UploadDate { get; set; }
}