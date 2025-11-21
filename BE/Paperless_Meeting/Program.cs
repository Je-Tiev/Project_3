using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.FileProviders;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Paperless_Meeting.Models;
using Paperless_Meeting.Data;
using Paperless_Meeting.Models;
using Paperless_Meeting.Interfaces.Auth;
using Paperless_Meeting.Services.Auth;
using Paperless_Meeting.Interfaces.Meeting;
using Paperless_Meeting.Services.Meeting;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cấu hình DbContext với PostgreSQL
=======
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
>>>>>>> feature/be-meetings-api
builder.Services.AddDbContext<PaperlessMeetingDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")
    ));

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// Cấu hình DI cho services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddAutoMapper(typeof(Program));

<<<<<<< HEAD
// Cấu hình JWT authentication
=======
>>>>>>> feature/be-meetings-api
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
<<<<<<< HEAD

// Test kết nối DB
app.MapGet("/test-db", async (PaperlessMeetingDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect 
            ? Results.Ok("✅ Kết nối đến PostgreSQL thành công!") 
            : Results.Problem("❌ Không thể kết nối đến database!");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Lỗi: {ex.Message}");
    }
});

=======
>>>>>>> feature/be-meetings-api
app.MapControllers();

app.Run();
