using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class Conclusion
{        
    [Key]
    public int ConclusionId { get; set; }

    [Required]
    public int MeetingId { get; set; }
    [ForeignKey("MeetingId")]
    public virtual Meeting Meeting { get; set; } = null!;
    
    [Required]
    public int ConclusionByUserId { get; set; }
    [ForeignKey("ConclusionByUserId")]
    public virtual User Creator { get; set; } = null!;

    [Required]
    public string Summary { get; set; } = string.Empty;
        
    [Required]
    public DateTime CreatedAt { get; set; }
}