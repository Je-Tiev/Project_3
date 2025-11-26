using Paperless_Meeting.DTOs.Auth;

namespace Paperless_Meeting.Repositories.Auth;

public interface IAuthService
{
    Task<string?> RegisterAsync(RegisterRequestDto registerDto);
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
}
