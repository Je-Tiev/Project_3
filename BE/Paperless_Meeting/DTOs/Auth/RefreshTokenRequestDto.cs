using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; }
}

