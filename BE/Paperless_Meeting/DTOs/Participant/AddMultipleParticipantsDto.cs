using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Participant;

public class AddMultipleParticipantsDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Danh sách participants phải có ít nhất 1 người")]
    public List<ParticipantDto> Participants { get; set; }
}

public class AddParticipantResultDto
{
    public int UserId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}
