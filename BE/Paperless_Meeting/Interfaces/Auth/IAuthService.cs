using Paperless_Meeting.DTOs.Auth;

namespace Paperless_Meeting.Interfaces.Auth;

public interface IAuthService
{
    Task<string?> RegisterAsync(RegisterRequestDto registerDto);

    Task<string?> LoginAsync(LoginRequestDto loginDto);
}