using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Auth;
using Paperless_Meeting.Interfaces.Auth;
using Paperless_Meeting.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Paperless_Meeting.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly PaperlessMeetingDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(PaperlessMeetingDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string?> RegisterAsync(RegisterRequestDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return "Tên đăng nhập đã tồn tại.";
            }

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return "Email đã được sử dụng.";
            }

            var user = new User
            {
                FullName = registerDto.FullName,
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                DepartmentId = registerDto.DepartmentID,
                Role = registerDto.Role
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> LoginAsync(LoginRequestDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            return CreateJwtToken(user);
        }
        
        private string CreateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}