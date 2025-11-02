using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Auth;
using Paperless_Meeting.Interfaces.Auth;

namespace Paperless_Meeting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto registerDto)
        {
            var errorMessage = await _authService.RegisterAsync(registerDto);

            if (errorMessage != null)
            {
                return BadRequest(errorMessage);
            }
            
            return Ok("Đăng ký thành công.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);

            if (response == null)
            {
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            return Ok(response);
        }
        
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto request)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (response == null)
            {
                return Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");
            }

            return Ok(response);
        }
        
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken(RefreshTokenRequestDto request)
        {
            var success = await _authService.RevokeTokenAsync(request.RefreshToken);

            if (!success)
            {
                return BadRequest("Không thể thu hồi token.");
            }

            return Ok("Token đã được thu hồi thành công.");
        }
    }
}