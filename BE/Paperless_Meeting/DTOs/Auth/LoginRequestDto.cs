using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}