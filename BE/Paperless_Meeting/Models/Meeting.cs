using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class Meeting
{
    [Key]
    public int MeetingId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    [Required]
    public int CreatedByUserId { get; set; }
    [ForeignKey("CreatedByUserId")]
    public virtual User Creator { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public enum MeetingStatus { Scheduled, InProgress, Finished, Cancelled }
    public MeetingStatus Status  { get; set; }
    
    public virtual ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    public virtual ICollection<Conclusion> Conclusions { get; set; } = new List<Conclusion>();
    public virtual ICollection<MeetingLog> Logs { get; set; } = new List<MeetingLog>();
    public virtual ICollection<MeetingAudio> Audios { get; set; } = new List<MeetingAudio>();
    public virtual ICollection<Poll> Polls { get; set; } = new List<Poll>();
}