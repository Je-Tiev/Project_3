using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class MeetingLog
{
    [Key]
    public int LogId { get; set; }

    [Required]
    public int MeetingId { get; set; }
    [ForeignKey("MeetingId")]
    public virtual Meeting Meeting { get; set; } = null!;
    
    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User UserAction { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    public DateTime ActionTime { get; set; }
}