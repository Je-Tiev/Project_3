using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.DTOs.Participant;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Models;

namespace Paperless_Meeting.Services.Meeting
{
    public class MeetingService : IMeetingService
    {
        private readonly PaperlessMeetingDbContext _context;

        public MeetingService(PaperlessMeetingDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách cuộc họp của user (là host hoặc participant)
        public async Task<List<MeetingListDto>> GetMeetingsByUserAsync(int userId)
        {
            return await _context.Meetings
                .Where(m => m.CreatedByUserId == userId || m.Participants.Any(p => p.UserId == userId))
                .Select(m => new MeetingListDto
                {
                    MeetingId = m.MeetingId,
                    Title = m.Title,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    Location = m.Location,
                    Status = m.Status.ToString()
                })
                .AsNoTracking()
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();
        }

        // 2. Lấy chi tiết cuộc họp + danh sách người tham gia (dùng để join room)
        public async Task<MeetingDetailDto?> GetMeetingByIdAsync(int meetingId)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Participants)
                    .ThenInclude(p => p.User)
                .Include(m => m.Documents)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MeetingId == meetingId);

            if (meeting == null) return null;

            return new MeetingDetailDto
            {
                MeetingId = meeting.MeetingId,
                Title = meeting.Title,
                Description = meeting.Description,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,
                Location = meeting.Location,
                Status = meeting.Status,
                CreatedByUserId = meeting.CreatedByUserId,
                CreatedAt = meeting.CreatedAt,
                Participants = meeting.Participants.Select(p => new ParticipantDto
                {
                    UserId = p.UserId,
                    FullName = p.User?.FullName ?? p.FullName ?? "Không xác định",
                    RoleInMeeting = p.RoleInMeeting,
                    AssignedTask = p.AssignedTask ?? "Chưa phân công",
                    IsPresent = p.IsPresent
                }).ToList()
            };
        }

        // 3. TẠO CUỘC HỌP
        public async Task<MeetingDetailDto> CreateMeetingAsync(MeetingCreateDto createDto, int creatorId)
        {
            var meeting = new Models.Meeting
            {
                Title = createDto.Title,
                Description = createDto.Description?.Trim() ?? string.Empty,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                Location = createDto.Location?.Trim() ?? string.Empty,
                CreatedByUserId = creatorId,
                Status = Models.Meeting.MeetingStatus.Scheduled,
                CreatedAt = DateTime.Now.ToUniversalTime()
            };

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync(); 

            // Lấy thông tin người tạo (host)
            var creator = await _context.Users.FindAsync(creatorId);
            _context.MeetingParticipants.Add(new MeetingParticipant
            {
                MeetingId = meeting.MeetingId,
                UserId = creatorId,
                FullName = creator?.FullName ?? "Quản trị viên",
                RoleInMeeting = MeetingParticipant.MeetingRole.Host,
                AssignedTask = "Chủ trì cuộc họp",
                IsPresent = false
            });

            // Thêm các người tham gia khác
            if (createDto.Participants != null)
            {
                foreach (var p in createDto.Participants)
                {
                    if (p.UserId == creatorId) continue;
                    var user = await _context.Users.FindAsync(p.UserId);
                    if (user == null) continue; 

                    _context.MeetingParticipants.Add(new MeetingParticipant
                    {
                        MeetingId = meeting.MeetingId,
                        UserId = p.UserId,
                        FullName = user.FullName,
                        RoleInMeeting = p.RoleInMeeting,
                        AssignedTask = string.IsNullOrWhiteSpace(p.AssignedTask)
                            ? "Tham gia thảo luận"
                            : p.AssignedTask.Trim(),
                        IsPresent = false
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Trả về chi tiết đầy đủ
            return await GetMeetingByIdAsync(meeting.MeetingId)
                   ?? throw new InvalidOperationException("Không thể lấy thông tin cuộc họp vừa tạo.");
        }

        // 4. CẬP NHẬT CUỘC HỌP
        public async Task<bool> UpdateMeetingAsync(int meetingId, MeetingUpdateDto updateDto)
        {
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.MeetingId == meetingId);

            if (meeting == null) return false;

            meeting.Title = updateDto.Title;
            meeting.Description = updateDto.Description ?.Trim() ?? string.Empty;
            meeting.StartTime = updateDto.StartTime;
            meeting.EndTime = updateDto.EndTime;
            meeting.Location = updateDto.Location ?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();
            return true;
        }

        // 5. XÓA CUỘC HỌP
        public async Task<bool> DeleteMeetingAsync(int meetingId)
        {
            var meeting = await _context.Meetings.FindAsync(meetingId);
            if (meeting == null) return false;

            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
            return true;
        }

        // 6. LẤY TẤT CẢ CUỘC HỌP (dành cho Admin)
        public async Task<IEnumerable<MeetingSummaryDto>> GetAllMeetingsAsync()
        {
            return await _context.Meetings
                .Select(m => new MeetingSummaryDto
                {
                    MeetingId = m.MeetingId,
                    Title = m.Title,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    Location = m.Location,
                    Status = m.Status
                })
                .AsNoTracking()
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();
        }
    }
}