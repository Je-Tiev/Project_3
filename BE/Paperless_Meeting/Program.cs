using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Paperless_Meeting.Models;
using Paperless_Meeting.Data;
using Paperless_Meeting.Hubs;
using Paperless_Meeting.Repositories.Auth;
using Paperless_Meeting.Repositories.Document;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.Participant;
using Paperless_Meeting.Services.Auth;
using Paperless_Meeting.Services.Document;
using Paperless_Meeting.Services.Meeting;
using Paperless_Meeting.Services.Participant;
using Paperless_Meeting.Services.Poll;

var builder = WebApplication.CreateBuilder(args);
// --- Bật logging đầy đủ cho console ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Cho phép JSON deserialize enum từ string
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
builder.Services.AddDbContext<PaperlessMeetingDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")
    ));

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524288000; // 500MB
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Cho phép gửi chi tiết lỗi về React
}); // Thêm SignalR

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            "https://52.221.241.199.nip.io",
            "http://localhost:3000",
            "http://localhost:5173",     
            "http://127.0.0.1:5173",
            "https://localhost:5173"     // nếu chạy FE trên https
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
    );
});

// Cấu hình DI cho services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IPollService, PollService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/meetinghub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
        };
    });

var app = builder.Build();

// Tạo database/tables trực tiếp nếu chưa tồn tại (dev helper).
// Lưu ý: EnsureCreated không dùng migrations và phù hợp cho môi trường dev/test.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaperlessMeetingDbContext>();
    // Nếu muốn dùng migrations thực sự, thay EnsureCreated bằng Migrate() và
    // đảm bảo migrations được tạo bằng `dotnet ef migrations add`.
    db.Database.EnsureCreated();
    // Seed initial users for development if none exist
    if (!db.Users.Any())
    {
        var admin = new User
        {
            FullName = "Quản Trị Viên",
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            DepartmentId = 1,
            Role = User.UserRole.Admin
        };
        var normalUser = new User
        {
            FullName = "Người Dùng",
            Username = "user",
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            DepartmentId = 2,
            Role = User.UserRole.User
        };
        db.Users.AddRange(admin, normalUser);
        db.SaveChanges();
    }
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var uploadPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/files",
    ServeUnknownFileTypes = true
});

// CORS cần đặt trước Authentication/Authorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
app.MapControllers();

app.MapHub<MeetingHub>("/meetinghub").RequireAuthorization(); // Map SignalR hub

app.Run();
