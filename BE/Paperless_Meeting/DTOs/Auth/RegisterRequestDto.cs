using Paperless_Meeting.Models;
using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
    
    [Required]
    public int DepartmentID { get; set; }

    //public UserRole Role { get; set; } = UserRole.ThanhVien;
    public User.UserRole Role { get; set; } = User.UserRole.NhanVien;
}