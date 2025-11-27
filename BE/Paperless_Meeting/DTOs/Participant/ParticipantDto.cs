using System.ComponentModel.DataAnnotations;
using Paperless_Meeting.Models;

namespace Paperless_Meeting.DTOs.Participant
{
    public class ParticipantCreateDto
    {
        [Required]
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public MeetingParticipant.MeetingRole RoleInMeeting { get; set; } = MeetingParticipant.MeetingRole.Participant;

        public string? AssignedTask { get; set; }
    }

    // Dùng để trả về (có FullName)
    public class ParticipantDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public MeetingParticipant.MeetingRole RoleInMeeting { get; set; }
        public string AssignedTask { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
    }
}