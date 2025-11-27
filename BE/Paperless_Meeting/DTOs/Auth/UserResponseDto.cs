using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Auth;

public class UserResponseDto
{
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Role { get; set; }

    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }
}
