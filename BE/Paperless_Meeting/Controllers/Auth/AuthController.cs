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
            var token = await _authService.LoginAsync(loginDto);

            if (token == null)
            {
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            return Ok(new { Token = token });
        }
    }
}