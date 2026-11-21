using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class MeetingChat
{
    [Key]
    public int ChatId { get; set; }

    [Required]
    public int MeetingId { get; set; }
    [ForeignKey("MeetingId")]
    public virtual Meeting Meeting { get; set; }

    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [Required]
    public string Message { get; set; }

    [Required]
    public DateTime SentAt { get; set; }
}
