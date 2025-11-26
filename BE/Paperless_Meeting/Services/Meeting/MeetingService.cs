using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.Interfaces.Meeting;
using Paperless_Meeting.Models;

namespace Paperless_Meeting.Services.Meeting
{
    public class MeetingService : IMeetingService
    {
        private readonly PaperlessMeetingDbContext _context;
        private readonly IMapper _mapper;

        public MeetingService(PaperlessMeetingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MeetingDetailDto> CreateMeetingAsync(MeetingCreateDto createDto, int creatorId)
        {
            // Tạo Meeting object thủ công (không dùng AutoMapper để tránh mapping ParticipantDto)
            var meeting = new Models.Meeting
            {
                Title = createDto.Title,
                Description = createDto.Description,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                Location = createDto.Location,
                CreatedByUserId = creatorId,
                Status = Models.Meeting.MeetingStatus.Scheduled,
                Participants = createDto.Participants
                    .Select((p, index) => new MeetingParticipant
                    {
                        UserId = p.UserId,
                        RoleInMeeting = p.RoleInMeeting,
                        // Sinh AssignedTask: Role + index
                        AssignedTask = $"{p.RoleInMeeting}_{index + 1}"
                    }).ToList()
            };

            await _context.Meetings.AddAsync(meeting);
            await _context.SaveChangesAsync();

            // Trả về DTO chi tiết
            meeting = await _context.Meetings
                .Include(m => m.Documents)
                .Include(m => m.Participants)
                .ThenInclude(mp => mp.User)
                .FirstAsync(m => m.MeetingId == meeting.MeetingId);

            return _mapper.Map<MeetingDetailDto>(meeting);
        }

        public async Task<IEnumerable<MeetingSummaryDto>> GetAllMeetingsAsync()
        {
            // ProjectTo là cách hiệu quả để chỉ query những cột cần thiết
            return await _context.Meetings
                .AsNoTracking()
                .ProjectTo<MeetingSummaryDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<MeetingDetailDto?> GetMeetingByIdAsync(int meetingId)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Participants) // Nạp danh sách người tham gia
                .ThenInclude(mp => mp.User) // Từ người tham gia, nạp thông tin User
                .Include(m => m.Documents) // Nạp danh sách tài liệu
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MeetingId == meetingId);

            if (meeting == null) return null;

            return _mapper.Map<MeetingDetailDto>(meeting);
        }
        
        public async Task<bool> UpdateMeetingAsync(int meetingId, MeetingCreateDto updateDto)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Participants)
                .FirstOrDefaultAsync(m => m.MeetingId == meetingId);

            if (meeting == null) return false;

            // Cập nhật các thuộc tính thủ công (không dùng AutoMapper)
            meeting.Title = updateDto.Title;
            meeting.Description = updateDto.Description;
            meeting.StartTime = updateDto.StartTime;
            meeting.EndTime = updateDto.EndTime;
            meeting.Location = updateDto.Location;

            // Xử lý cập nhật danh sách người tham gia (xóa cũ, thêm mới)
            meeting.Participants.Clear();
            var participantList = updateDto.Participants.ToList();
            for (int i = 0; i < participantList.Count; i++)
            {
                meeting.Participants.Add(new MeetingParticipant
                {
                    UserId = participantList[i].UserId,
                    RoleInMeeting = participantList[i].RoleInMeeting,
                    // Sinh AssignedTask: Role + index
                    AssignedTask = $"{participantList[i].RoleInMeeting}_{i + 1}"
                });
            }

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
    }
}