using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class Poll
{
    [Key]
    public int PollId { get; set; }

    [Required]
    public int MeetingId { get; set; }
    [ForeignKey("MeetingId")]
    public virtual Meeting Meeting { get; set; }

    [Required]
    public int VoteByUserId { get; set; }
    [ForeignKey("VoteByUserId")]
    public virtual User VotedUser { get; set; }

    [Required]
    public string Question { get; set; }

    public enum PollStatus { Open, Closed }
    [Required]
    public PollStatus Status { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
    
    public virtual ICollection<UserVote> UserVotes { get; set; }
}