using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Auth;
using Paperless_Meeting.Repositories.Auth;

namespace Paperless_Meeting.Controllers.Auth
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

        [HttpPost("register/user")]
        public async Task<IActionResult> RegisterUser(RegisterRequestDto registerDto)
        {
            var errorMessage = await _authService.RegisterAsync(registerDto);

            if (errorMessage != null)
            {
                return BadRequest(errorMessage);
            }

            return Ok("Đăng ký thành công.");
        }

        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterRequestDto registerDto)
        {
            var errorMessage = await _authService.RegisterAdminAsync(registerDto);

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

        [HttpGet("users")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách user", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin user", error = ex.Message });
            }
        }

        [HttpPut("profile/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(int id, UpdateUserDto updateDto)
        {
            try
            {
                // Người dùng chỉ có thể cập nhật hồ sơ của chính mình
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || int.Parse(userIdClaim.Value) != id)
                {
                    return Forbid("Bạn chỉ có thể cập nhật hồ sơ của chính mình.");
                }

                var errorMessage = await _authService.UpdateUserProfileAsync(id, updateDto);

                if (errorMessage != null)
                {
                    return BadRequest(new { message = errorMessage });
                }

                var user = await _authService.GetUserByIdAsync(id);
                return Ok(new { message = "Cập nhật hồ sơ thành công.", user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật hồ sơ", error = ex.Message });
            }
        }

        [HttpPut("admin/users/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> AdminUpdateUser(int id, UpdateUserDto updateDto)
        {
            try
            {
                var errorMessage = await _authService.AdminUpdateUserAsync(id, updateDto);

                if (errorMessage != null)
                {
                    return BadRequest(new { message = errorMessage });
                }

                var user = await _authService.GetUserByIdAsync(id);
                return Ok(new { message = "Cập nhật thông tin người dùng thành công.", user });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "Đã xảy ra lỗi khi cập nhật thông tin người dùng", error = ex.Message });
            }
        }

        [HttpPut("admin/users/{id}/role")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> AdminUpdateUserRole(int id, [FromBody] UpdateUserRoleDto updateRoleDto)
        {
            try
            {
                var errorMessage = await _authService.AdminUpdateUserRoleAsync(id, updateRoleDto.Role);

                if (errorMessage != null)
                {
                    return BadRequest(new { message = errorMessage });
                }

                var user = await _authService.GetUserByIdAsync(id);
                return Ok(new { message = "Cập nhật vai trò người dùng thành công.", user });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "Đã xảy ra lỗi khi cập nhật vai trò người dùng", error = ex.Message });
            }
        }
    }
}