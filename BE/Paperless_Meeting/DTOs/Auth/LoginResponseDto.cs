namespace Paperless_Meeting.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public string Role { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public DateTime TokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}

