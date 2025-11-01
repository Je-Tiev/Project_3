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
    public virtual Meeting Meeting { get; set; }
    
    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User UserAction { get; set; }

    [Required]
    [StringLength(100)]
    public string Action { get; set; }

    [Required]
    public DateTime ActionTime { get; set; }
}