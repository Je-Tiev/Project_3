using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Models;

namespace Paperless_Meeting.Data;

public class PaperlessMeetingDbContext : DbContext
{
    public PaperlessMeetingDbContext (DbContextOptions<PaperlessMeetingDbContext> options)
        : base(options)
    {
    }
    public DbSet<Paperless_Meeting.Models.Conclusion> Conclusions { get; set; }
    public DbSet<Paperless_Meeting.Models.Department> Departments { get; set; }
    public DbSet<Paperless_Meeting.Models.Document> Documents { get; set; }
    public DbSet<Paperless_Meeting.Models.MeetingAudio> MeetingAudios { get; set; }
    public DbSet<Paperless_Meeting.Models.MeetingLog> MeetingLogs { get; set; }
    public DbSet<Paperless_Meeting.Models.MeetingParticipant> MeetingParticipants { get; set; }
    public DbSet<Paperless_Meeting.Models.Meeting> Meetings { get; set; }
    public DbSet<Paperless_Meeting.Models.Note> Notes { get; set; }
    public DbSet<Paperless_Meeting.Models.Poll> Polls { get; set; }
    public DbSet<Paperless_Meeting.Models.User> Users { get; set; }
    public DbSet<Paperless_Meeting.Models.UserVote> UserVotes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình composite key cho MeetingParticipant
        modelBuilder.Entity<MeetingParticipant>()
            .HasKey(mp => new { mp.MeetingId, mp.UserId });

        // Seed data cho Departments
        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, DepartmentName = "Phòng Bảo vệ" },
            new Department { DepartmentId = 2, DepartmentName = "Phòng Kỹ thuật" },
            new Department { DepartmentId = 3, DepartmentName = "Phòng Kế toán" },
            new Department { DepartmentId = 4, DepartmentName = "Ban Giám đốc" },
            new Department { DepartmentId = 5, DepartmentName = "Phòng Nhân sự" }
        );
    }
}