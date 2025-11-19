using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Paperless_Meeting.Services
{
    public interface IHmsService
    {
        Task<string> CreateRoomAndGetJoinUrlAsync(string title, string userId, string role = "host");
    }

    public class HmsService : IHmsService
    {
        private readonly HttpClient _http;
        private readonly string _accessKey; 
        private readonly string _secret;
        private readonly string _templateId;

        public HmsService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _accessKey = config["Hms:AppAccessKey"]!;
            _secret = config["Hms:AppSecret"] ?? "";
            _templateId = config["Hms:TemplateId"]!;

            _http.BaseAddress = new Uri("https://api.100ms.live/v2/");
            _http.DefaultRequestHeaders.Authorization = new("Bearer", _accessKey);
        }

        private async Task<string> CreateRoomAsync(string title)
        {
            var payload = new
            {
                name = title,
                description = "Paperless Meeting .NET 8",
                template_id = _templateId
            };

            var response = await _http.PostAsJsonAsync("rooms", payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"100ms error: {response.StatusCode} - {content}");

            var json = JsonDocument.Parse(content).RootElement;
            return json.GetProperty("id").GetString()!;
        }

        private string GenerateClientToken(string roomId, string userId, string role = "host")
        {
            var claims = new[]
            {
                new Claim("room_id", roomId),
                new Claim("user_id", userId),
                new Claim("role", role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> CreateRoomAndGetJoinUrlAsync(string title, string userId, string role = "host")
        {
            var roomId = await CreateRoomAsync(title);
            var token = GenerateClientToken(roomId, userId, role);
            return $"https://app.100ms.live/prebuilt/{roomId}?token={token}";
            // return $"https://prod-in.100ms.live/hmsapp/{roomId}?token={token}&skip_preview=true";
        }
    }
}