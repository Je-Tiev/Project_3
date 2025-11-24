using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class Note
{
    [Key]
    public int NoteId { get; set; }

    [Required]
    public int MeetingId { get; set; }
    [ForeignKey("MeetingId")]
    public virtual Meeting Meeting { get; set; }
        
    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User UserNote { get; set; }
        
    [Required]
    public string Content { get; set; }
        
    [Required]
    public DateTime CreatedAt { get; set; }
}