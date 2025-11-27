using Paperless_Meeting.DTOs.Auth;
using Paperless_Meeting.DTOs.Participant;
namespace Paperless_Meeting.Repositories.Auth;

public interface IAuthService
{
    Task<string?> RegisterAsync(RegisterRequestDto registerDto);
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(int userId);

}