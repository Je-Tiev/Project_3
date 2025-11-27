using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class MeetingParticipant
{
    public int MeetingParticipantId { get; set; }
    public int MeetingId { get; set; }
    
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string AssignedTask { get; set; } = string.Empty;

    public bool IsPresent { get; set; } = false;
    public MeetingRole RoleInMeeting { get; set; } = MeetingRole.Participant;

    public enum MeetingRole { Host = 1, Secretary = 2, Participant = 3 }
    
    [ForeignKey("MeetingId")]
    public Meeting Meeting { get; set; } = null!;
    public User User { get; set; } = null!;
}