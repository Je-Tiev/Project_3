using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class MeetingParticipant
{
    [Required]
    [Column(Order = 0)]
    public int MeetingId { get; set; }
    [ForeignKey("MeetingId")]
    public virtual Meeting Meeting { get; set; }

    [Required]
    [Column(Order = 1)]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    
    public bool IsPresent { get; set; } = false;
    
    public string? AssignedTask { get; set; }

    public enum MeetingRole { Host, Member, Guest }
    public MeetingRole RoleInMeeting  { get; set; }
}