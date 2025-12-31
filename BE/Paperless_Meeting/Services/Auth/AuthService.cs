using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Auth;
using Paperless_Meeting.Repositories.Auth;
using Paperless_Meeting.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
                Role = User.UserRole.User
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> RegisterAdminAsync(RegisterRequestDto registerDto)
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
                Role = User.UserRole.Admin
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return null;
        }
        
        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            // Tạo JWT token
            var tokenExpiry = DateTime.UtcNow.AddHours(1); // Access token hết hạn sau 1 giờ
            var token = CreateJwtToken(user, tokenExpiry);
            
            // Tạo refresh token
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Refresh token hết hạn sau 7 ngày
            
            // Lưu refresh token vào database
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshTokenExpiry,
                IsRevoked = false
            };
            
            await _context.RefreshTokens.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();
            
            // Trả về response với token và thông tin user
            return new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Role = user.Role.ToString(),
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                TokenExpiresAt = tokenExpiry,
                RefreshTokenExpiresAt = refreshTokenExpiry
            };
        }
        
        public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            // Tìm refresh token trong database
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            
            // Kiểm tra token có tồn tại và còn hợp lệ không
            if (storedToken == null || !storedToken.IsActive)
            {
                return null;
            }
            
            // Thu hồi refresh token cũ
            storedToken.IsRevoked = true;
            
            // Tạo JWT token mới
            var tokenExpiry = DateTime.UtcNow.AddHours(1);
            var newToken = CreateJwtToken(storedToken.User, tokenExpiry);
            
            // Tạo refresh token mới
            var newRefreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            
            // Lưu refresh token mới
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = storedToken.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshTokenExpiry,
                IsRevoked = false
            };
            
            await _context.RefreshTokens.AddAsync(newRefreshTokenEntity);
            await _context.SaveChangesAsync();
            
            return new LoginResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Role = storedToken.User.Role.ToString(),
                UserId = storedToken.User.UserId,
                Username = storedToken.User.Username,
                FullName = storedToken.User.FullName,
                TokenExpiresAt = tokenExpiry,
                RefreshTokenExpiresAt = refreshTokenExpiry
            };
        }
        
        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            
            if (storedToken == null || storedToken.IsRevoked)
            {
                return false;
            }
            
            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .ToListAsync();

            return users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.ToString(),
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department?.DepartmentName
            });
        }
        
        public async Task<UserResponseDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return null;
            }

            return new UserResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.DepartmentName
            };
        }

        public async Task<string?> UpdateUserProfileAsync(int userId, UpdateUserDto updateDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return "Người dùng không tồn tại.";
            }

            // Kiểm tra email nếu được cập nhật
            if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == updateDto.Email))
                {
                    return "Email đã được sử dụng.";
                }
                user.Email = updateDto.Email;
            }

            // Cập nhật FullName
            if (!string.IsNullOrWhiteSpace(updateDto.FullName))
            {
                user.FullName = updateDto.FullName;
            }

            // Cập nhật Department
            if (updateDto.DepartmentId.HasValue)
            {
                var department = await _context.Set<Department>().FindAsync(updateDto.DepartmentId);
                if (department == null)
                {
                    return "Bộ phận không tồn tại.";
                }
                user.DepartmentId = updateDto.DepartmentId.Value;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return null;
        }

        public async Task<string?> AdminUpdateUserAsync(int userId, UpdateUserDto updateDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return "Người dùng không tồn tại.";
            }

            // Kiểm tra email nếu được cập nhật
            if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == updateDto.Email && u.UserId != userId))
                {
                    return "Email đã được sử dụng.";
                }
                user.Email = updateDto.Email;
            }

            // Cập nhật FullName
            if (!string.IsNullOrWhiteSpace(updateDto.FullName))
            {
                user.FullName = updateDto.FullName;
            }

            // Cập nhật Department
            if (updateDto.DepartmentId.HasValue)
            {
                var department = await _context.Set<Department>().FindAsync(updateDto.DepartmentId);
                if (department == null)
                {
                    return "Bộ phận không tồn tại.";
                }
                user.DepartmentId = updateDto.DepartmentId.Value;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return null;
        }

        public async Task<string?> AdminUpdateUserRoleAsync(int userId, string role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return "Người dùng không tồn tại.";
            }

            // Kiểm tra role hợp lệ
            if (!Enum.TryParse<User.UserRole>(role, true, out var userRole))
            {
                return "Role không hợp lệ. Các role hợp lệ: User, Admin, SuperAdmin.";
            }

            user.Role = userRole;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return null;
        }
        
        private string CreateJwtToken(User user, DateTime expiry)
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
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}