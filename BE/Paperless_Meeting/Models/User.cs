using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless_Meeting.Models;

public class User
{
    [Key]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression("^[a-zA-Z0-9_.-]{5,}$")]
    public string Username { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    public enum UserRole { SuperAdmin, Admin, User }
    [Required]
    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; }

    [Required]
    public int DepartmentId { get; set; }
    public virtual Department Department { get; set; }

    public virtual ICollection<MeetingParticipant> MeetingParticipations { get; set; }
    public virtual ICollection<UserVote> UserVotes { get; set; }
}