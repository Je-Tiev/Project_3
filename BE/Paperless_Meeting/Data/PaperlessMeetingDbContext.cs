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
    public DbSet<Paperless_Meeting.Models.Room> Rooms { get; set; }
    
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

        // Chống Double Voting: Ràng buộc duy nhất cho một cặp (PollId, UserId)
        modelBuilder.Entity<UserVote>()
            .HasIndex(uv => new { uv.PollId, uv.UserId })
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tối ưu hóa tra cứu Refresh Token
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token);

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

        // Liên kết Phòng họp vật lý (PhysicalRoom) với Meeting
        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.PhysicalRoom)
            .WithMany(r => r.Meetings)
            .HasForeignKey(m => m.PhysicalRoomId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed danh mục phòng họp vật lý ban đầu
        modelBuilder.Entity<Room>().HasData(
            new Room
            {
                RoomId = 1,
                RoomName = "Conference Room A",
                Location = "Tòa nhà A - Tầng 2",
                Capacity = 30,
                Equipment = "Màn hình LED, Hệ thống âm thanh, Camera 360",
                Status = RoomStatus.Available,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Room
            {
                RoomId = 2,
                RoomName = "Meeting Room 3",
                Location = "Tòa nhà B - Tầng 3",
                Capacity = 15,
                Equipment = "Máy chiếu, Bảng tương tác",
                Status = RoomStatus.InUse,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Room
            {
                RoomId = 3,
                RoomName = "Boardroom",
                Location = "Tòa nhà C - Tầng 5",
                Capacity = 50,
                Equipment = "Hội nghị truyền hình 4K, Micro đại biểu",
                Status = RoomStatus.Available,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}