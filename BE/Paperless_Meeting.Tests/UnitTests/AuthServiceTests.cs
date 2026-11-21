using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Auth;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Auth;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class AuthServiceTests
    {
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:Key", "SUPER_SECURE_TEST_JWT_SECRET_KEY_EXCEEDING_32_BYTES_12345" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        private PaperlessMeetingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PaperlessMeetingDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_ValidData_HashesPasswordAndSavesUser()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var service = new AuthService(context, _configuration);
            var dto = new RegisterRequestDto
            {
                FullName = "Nguyen Van A",
                Username = "nguyenvana",
                Email = "a@bachkhoa.edu.vn",
                Password = "Password123!",
                DepartmentID = 1
            };

            // Act
            var error = await service.RegisterAsync(dto);

            // Assert
            error.Should().BeNull();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == "nguyenvana");
            user.Should().NotBeNull();
            user!.FullName.Should().Be("Nguyen Van A");
            BCrypt.Net.BCrypt.Verify("Password123!", user.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUsername_ReturnsError()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.Users.Add(new User
            {
                Username = "existinguser",
                FullName = "Existing",
                Email = "existing@hust.edu.vn",
                PasswordHash = "hash",
                DepartmentId = 1,
                Role = User.UserRole.User
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, _configuration);
            var dto = new RegisterRequestDto
            {
                Username = "existinguser",
                FullName = "New",
                Email = "new@hust.edu.vn",
                Password = "pass",
                DepartmentID = 1
            };

            // Act
            var error = await service.RegisterAsync(dto);

            // Assert
            error.Should().Be("Tên đăng nhập đã tồn tại.");
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsJwtAndRefreshToken()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("SecretPassword123");
            context.Users.Add(new User
            {
                UserId = 1,
                Username = "user1",
                FullName = "User One",
                Email = "user1@hust.edu.vn",
                PasswordHash = hashedPassword,
                DepartmentId = 1,
                Role = User.UserRole.User
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, _configuration);
            var loginDto = new LoginRequestDto
            {
                Username = "user1",
                Password = "SecretPassword123"
            };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();
            result.Username.Should().Be("user1");
            result.Role.Should().Be("User");

            var savedRefreshToken = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == result.RefreshToken);
            savedRefreshToken.Should().NotBeNull();
            savedRefreshToken!.UserId.Should().Be(1);
            savedRefreshToken.IsRevoked.Should().BeFalse();
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ReturnsNull()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.Users.Add(new User
            {
                UserId = 2,
                Username = "user2",
                FullName = "User Two",
                Email = "user2@hust.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
                DepartmentId = 1,
                Role = User.UserRole.User
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, _configuration);

            // Act
            var result = await service.LoginAsync(new LoginRequestDto
            {
                Username = "user2",
                Password = "WrongPassword"
            });

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_ActiveToken_RotatesTokenAndRevokesOld()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var user = new User
            {
                UserId = 3,
                Username = "user3",
                FullName = "User Three",
                Email = "user3@hust.edu.vn",
                PasswordHash = "hash",
                DepartmentId = 1,
                Role = User.UserRole.User
            };
            context.Users.Add(user);

            var oldToken = "old_valid_refresh_token_string";
            context.RefreshTokens.Add(new RefreshToken
            {
                Id = 1,
                UserId = 3,
                Token = oldToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, _configuration);

            // Act
            var result = await service.RefreshTokenAsync(oldToken);

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBe(oldToken);

            // Verify old token is marked revoked
            var oldEntity = await context.RefreshTokens.FirstAsync(rt => rt.Token == oldToken);
            oldEntity.IsRevoked.Should().BeTrue();

            // Verify new token exists and is active
            var newEntity = await context.RefreshTokens.FirstAsync(rt => rt.Token == result.RefreshToken);
            newEntity.IsRevoked.Should().BeFalse();
            newEntity.UserId.Should().Be(3);
        }

        [Fact]
        public async Task RefreshTokenAsync_RevokedToken_ReturnsNull()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.RefreshTokens.Add(new RefreshToken
            {
                Id = 2,
                UserId = 4,
                Token = "revoked_token",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(5),
                IsRevoked = true
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, _configuration);

            // Act
            var result = await service.RefreshTokenAsync("revoked_token");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RevokeTokenAsync_ActiveToken_MarksRevokedAndReturnsTrue()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.RefreshTokens.Add(new RefreshToken
            {
                Id = 3,
                UserId = 5,
                Token = "token_to_revoke",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, _configuration);

            // Act
            var success = await service.RevokeTokenAsync("token_to_revoke");

            // Assert
            success.Should().BeTrue();
            var token = await context.RefreshTokens.FirstAsync(rt => rt.Token == "token_to_revoke");
            token.IsRevoked.Should().BeTrue();
        }
    }
}
