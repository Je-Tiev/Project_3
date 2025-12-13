using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Poll;
public class PollVoteRequestDto
{
    [Required]
    public bool Choice { get; set; }
}