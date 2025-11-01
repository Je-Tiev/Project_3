using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class UserVote
{
    [Key]
    public int VoteId { get; set; }

    [Required]
    public int PollId { get; set; }
    [ForeignKey("PollId")]
    public virtual Poll Poll { get; set; }

    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    
    [Required]
    public bool Choice { get; set; }

    [Required]
    public DateTime VotedAt { get; set; }
}