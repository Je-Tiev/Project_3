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
    public virtual Meeting Meeting { get; set; }
    
    [Required]
    public int ConclusionByUserId { get; set; }
    [ForeignKey("ConclusionByUserId")]
    public virtual User Creator { get; set; }

    [Required]
    public string Summary { get; set; }
        
    [Required]
    public DateTime CreatedAt { get; set; }
}