using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class MeetingAudio
{
    [Key]
    public int AudioId { get; set; }

    [Required]
    public int MeetingId { get; set; }
    [ForeignKey("MeetingId")]
    public virtual Meeting Meeting { get; set; } = null!;

    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User Speaker { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string AudioPath { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; }

    [Range(0, double.MaxValue)]
    public float Duration { get; set; }
        
    public string TranscribedText { get; set; } = string.Empty;
}