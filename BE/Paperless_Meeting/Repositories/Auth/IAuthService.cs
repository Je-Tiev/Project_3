using Paperless_Meeting.DTOs.Auth;

namespace Paperless_Meeting.Repositories.Auth;

public interface IAuthService
{
    Task<string?> RegisterAsync(RegisterRequestDto registerDto);
    Task<string?> RegisterAdminAsync(RegisterRequestDto registerDto);
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(int userId);
    Task<string?> UpdateUserProfileAsync(int userId, UpdateUserDto updateDto);
    Task<string?> AdminUpdateUserAsync(int userId, UpdateUserDto updateDto);
    Task<string?> AdminUpdateUserRoleAsync(int userId, string role);
}
