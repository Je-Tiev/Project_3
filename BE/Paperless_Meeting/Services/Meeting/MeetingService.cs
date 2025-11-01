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
            // Dùng AutoMapper để chuyển DTO sang Model
            var meeting = _mapper.Map<Models.Meeting>(createDto);

            // Gán các giá trị không có trong DTO
            meeting.CreatedByUserId = creatorId;
            meeting.Status = Models.Meeting.MeetingStatus.Scheduled;

            // Xử lý danh sách người tham gia
            meeting.Participants = createDto.Participants
                .Select(p => new MeetingParticipant
                {
                    UserId = p.UserId,
                    RoleInMeeting = p.RoleInMeeting
                }).ToList();

            await _context.Meetings.AddAsync(meeting);
            await _context.SaveChangesAsync();

            // Trả về DTO chi tiết
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

            // Dùng AutoMapper để cập nhật các thuộc tính từ DTO vào entity đã có
            _mapper.Map(updateDto, meeting);

            // Xử lý cập nhật danh sách người tham gia (xóa cũ, thêm mới)
            meeting.Participants.Clear();
            foreach (var participantDto in updateDto.Participants)
            {
                meeting.Participants.Add(new MeetingParticipant
                {
                    UserId = participantDto.UserId,
                    RoleInMeeting = participantDto.RoleInMeeting
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