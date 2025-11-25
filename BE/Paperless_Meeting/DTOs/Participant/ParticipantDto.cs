using System.ComponentModel.DataAnnotations;
using Paperless_Meeting.Models;

namespace Paperless_Meeting.DTOs.Participant;

public class ParticipantDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public MeetingParticipant.MeetingRole RoleInMeeting { get; set; }
}