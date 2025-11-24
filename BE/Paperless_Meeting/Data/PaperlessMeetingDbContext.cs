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
    public DbSet<Paperless_Meeting.Models.RefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình composite key cho MeetingParticipant
        modelBuilder.Entity<MeetingParticipant>()
            .HasKey(mp => new { mp.MeetingId, mp.UserId });

        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(mp => mp.Meeting)
            .WithMany(m => m.Participants)
            .HasForeignKey(mp => mp.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(mp => mp.User)
            .WithMany(u => u.MeetingParticipations)
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.Creator)
            .WithMany()
            .HasForeignKey(m => m.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasOne(d => d.Uploader)
            .WithMany()
            .HasForeignKey(d => d.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Note>()
            .HasOne(n => n.UserNote)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MeetingLog>()
            .HasOne(ml => ml.UserAction)
            .WithMany()
            .HasForeignKey(ml => ml.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MeetingAudio>()
            .HasOne(ma => ma.Speaker)
            .WithMany()
            .HasForeignKey(ma => ma.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Poll>()
            .HasOne(p => p.VotedUser)
            .WithMany()
            .HasForeignKey(p => p.VoteByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Conclusion>()
            .HasOne(c => c.Creator)
            .WithMany()
            .HasForeignKey(c => c.ConclusionByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserVote>()
            .HasOne(uv => uv.User)
            .WithMany(u => u.UserVotes)
            .HasForeignKey(uv => uv.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Members)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    
        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, DepartmentName = "Phòng Bảo vệ" },
            new Department { DepartmentId = 2, DepartmentName = "Phòng Kỹ thuật" },
            new Department { DepartmentId = 3, DepartmentName = "Phòng Kế toán" },
            new Department { DepartmentId = 4, DepartmentName = "Ban Giám đốc" },
            new Department { DepartmentId = 5, DepartmentName = "Phòng Nhân sự" }
        );
    }
}