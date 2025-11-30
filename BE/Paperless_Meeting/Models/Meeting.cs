using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class Meeting
{
    [Key]
    public int MeetingId { get; set; }
    public string RoomId { get; set; } = string.Empty;
    [Required]
    [StringLength(200)]
    public string Title { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
    // Notes: EndTime > StartTime

    [Required]
    [StringLength(200)]
    public string Location { get; set; }
    
    [Required]
    public int CreatedByUserId { get; set; }
    [ForeignKey("CreatedByUserId")]
    public virtual User Creator { get; set; }
    
    public enum MeetingStatus { Scheduled, InProgress, Finished, Cancelled }
    public MeetingStatus Status  { get; set; }
    
    public virtual ICollection<MeetingParticipant> Participants { get; set; }
    public virtual ICollection<Document> Documents { get; set; }
    public virtual ICollection<Note> Notes { get; set; }
    public virtual ICollection<Conclusion> Conclusions { get; set; }
    public virtual ICollection<MeetingLog> Logs { get; set; }
    public virtual ICollection<MeetingAudio> Audios { get; set; }
    public virtual ICollection<Poll> Polls { get; set; }
}