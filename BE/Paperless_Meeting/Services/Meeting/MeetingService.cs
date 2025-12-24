using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Meeting;
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

        public async Task<List<MeetingListDto>> GetMeetingsByUserAsync(int userId)
        {
            var meetings = await _context.Meetings
                .Where(m => m.Participants.Any(p => p.UserId == userId) || m.CreatedByUserId == userId)
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
                .ToListAsync();

            return meetings;
        }

        public async Task<MeetingDetailDto?> GetMeetingByIdAsync(int meetingId)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Creator)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MeetingId == meetingId);

            if (meeting == null) return null;

            return new MeetingDetailDto
            {
                MeetingId = meeting.MeetingId,
                RoomId = meeting.RoomId,
                Title = meeting.Title,
                Description = meeting.Description,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,
                Location = meeting.Location,
                Status = meeting.Status,
                CreatedByUserId = meeting.CreatedByUserId,
                CreatedByUserName = meeting.Creator?.FullName
            };
        }

        public async Task<MeetingDetailDto> CreateMeetingAsync(MeetingCreateDto createDto, int creatorId)
        {
            var meeting = new Models.Meeting
            {
                Title = createDto.Title,
                RoomId = createDto.RoomId,
                Description = createDto.Description,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                Location = createDto.Location,
                CreatedByUserId = creatorId,
                Status = Models.Meeting.MeetingStatus.Scheduled
            };

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            var creatorParticipant = new MeetingParticipant
            {
                MeetingId = meeting.MeetingId,
                UserId = creatorId,
                RoleInMeeting = MeetingParticipant.MeetingRole.Host,
                IsPresent = false
            };

            _context.MeetingParticipants.Add(creatorParticipant);
            await _context.SaveChangesAsync();

            return new MeetingDetailDto
            {
                MeetingId = meeting.MeetingId,
                RoomId = meeting.RoomId,
                Title = meeting.Title,
                Description = meeting.Description,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,
                Location = meeting.Location,
                Status = meeting.Status,
                CreatedByUserId = meeting.CreatedByUserId,
                CreatedByUserName = meeting.Creator?.FullName
            };
        }

        public async Task<bool> UpdateMeetingAsync(int meetingId, MeetingUpdateDto updateDto)
        {
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.MeetingId == meetingId);

            if (meeting == null) return false;

            meeting.Title = updateDto.Title;
            meeting.Description = updateDto.Description;
            meeting.StartTime = updateDto.StartTime;
            meeting.EndTime = updateDto.EndTime;
            meeting.Location = updateDto.Location;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMeetingAsync(int meetingId)
        {
            var meeting = await _context.Meetings.FindAsync(meetingId);
            if (meeting == null) return false;

            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<MeetingSummaryDto>> GetAllMeetingsAsync()
        {
            var meetings = await _context.Meetings
                .Include(m => m.Creator)
                .Select(m => new MeetingSummaryDto
                {
                    MeetingId = m.MeetingId,
                    Title = m.Title,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    Location = m.Location,
                    Status = m.Status,
                    CreatedByUserId = m.CreatedByUserId,
                    CreatedByUserName = m.Creator.FullName
                })
                .AsNoTracking()
                .ToListAsync();

            return meetings;
        }
        
        public async Task<bool> ExtendMeetingAsync(int meetingId, int userId)
        {
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.MeetingId == meetingId);

            if (meeting == null) return false;

            // Kiểm tra xem người dùng có phải là Host của cuộc họp không
            if (meeting.CreatedByUserId != userId) return false;

            // Gia hạn thêm 1 tiếng
            meeting.EndTime = meeting.EndTime.AddHours(1);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}