using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Room;
using Paperless_Meeting.Models;
using Paperless_Meeting.Repositories.Room;

namespace Paperless_Meeting.Services.Room
{
    public class RoomService : IRoomService
    {
        private readonly PaperlessMeetingDbContext _context;

        public RoomService(PaperlessMeetingDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomDto>> GetAllRoomsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfToday = now.Date;
            var endOfToday = startOfToday.AddDays(1);

            var rooms = await _context.Rooms
                .Include(r => r.Meetings.Where(m => m.Status != Models.Meeting.MeetingStatus.Cancelled && m.EndTime >= startOfToday))
                    .ThenInclude(m => m.Creator)
                .AsNoTracking()
                .ToListAsync();

            return rooms.Select(r => MapToDto(r, now, startOfToday, endOfToday)).ToList();
        }

        public async Task<RoomDto?> GetRoomByIdAsync(int id)
        {
            var now = DateTime.UtcNow;
            var startOfToday = now.Date;
            var endOfToday = startOfToday.AddDays(1);

            var room = await _context.Rooms
                .Include(r => r.Meetings.Where(m => m.Status != Models.Meeting.MeetingStatus.Cancelled && m.EndTime >= startOfToday))
                    .ThenInclude(m => m.Creator)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null) return null;

            return MapToDto(room, now, startOfToday, endOfToday);
        }

        public async Task<RoomDto> CreateRoomAsync(RoomCreateDto createDto)
        {
            var roomStatus = RoomStatus.Available;
            if (!string.IsNullOrWhiteSpace(createDto.Status))
            {
                Enum.TryParse<RoomStatus>(createDto.Status, true, out roomStatus);
            }

            var room = new Models.Room
            {
                RoomName = createDto.RoomName,
                Location = createDto.Location,
                Capacity = createDto.Capacity,
                Equipment = createDto.Equipment,
                Status = roomStatus,
                CreatedAt = DateTime.UtcNow
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return new RoomDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                Location = room.Location,
                Capacity = room.Capacity,
                Equipment = room.Equipment,
                Status = room.Status.ToString(),
                CreatedAt = room.CreatedAt,
                CurrentUser = "-",
                UpcomingSchedule = "No upcoming meetings"
            };
        }

        public async Task<bool> UpdateRoomAsync(int id, RoomCreateDto updateDto)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return false;

            room.RoomName = updateDto.RoomName;
            room.Location = updateDto.Location;
            room.Capacity = updateDto.Capacity;
            room.Equipment = updateDto.Equipment;

            if (!string.IsNullOrWhiteSpace(updateDto.Status) && Enum.TryParse<RoomStatus>(updateDto.Status, true, out var parsedStatus))
            {
                room.Status = parsedStatus;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Meetings.Where(m => m.Status == Models.Meeting.MeetingStatus.Scheduled || m.Status == Models.Meeting.MeetingStatus.InProgress))
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null) return false;

            // Không cho xóa nếu có cuộc họp đang hoặc sắp diễn ra trong phòng này
            if (room.Meetings.Any()) return false;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeMeetingId = null)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null || room.Status == RoomStatus.Maintenance)
            {
                return false;
            }

            var query = _context.Meetings.Where(m =>
                m.PhysicalRoomId == roomId &&
                m.Status != Models.Meeting.MeetingStatus.Cancelled &&
                m.StartTime < endTime &&
                m.EndTime > startTime);

            if (excludeMeetingId.HasValue)
            {
                query = query.Where(m => m.MeetingId != excludeMeetingId.Value);
            }

            var hasConflict = await query.AnyAsync();
            return !hasConflict;
        }

        private static RoomDto MapToDto(Models.Room room, DateTime now, DateTime startOfToday, DateTime endOfToday)
        {
            // Kiểm tra cuộc họp đang diễn ra ngay lúc này
            var currentMeeting = room.Meetings
                .FirstOrDefault(m => m.StartTime <= now && m.EndTime >= now);

            string statusStr;
            string currentUser = "-";

            if (room.Status == RoomStatus.Maintenance)
            {
                statusStr = "Maintenance";
            }
            else if (currentMeeting != null)
            {
                statusStr = "In Use";
                currentUser = currentMeeting.Creator?.FullName ?? $"User #{currentMeeting.CreatedByUserId}";
            }
            else
            {
                statusStr = "Available";
            }

            // Lịch sắp tới trong ngày hôm nay
            var todayMeetings = room.Meetings
                .Where(m => m.StartTime >= now && m.StartTime < endOfToday)
                .OrderBy(m => m.StartTime)
                .ToList();

            string upcoming;
            if (todayMeetings.Count == 0)
            {
                upcoming = "No upcoming meetings";
            }
            else
            {
                var next = todayMeetings.First();
                var diff = next.StartTime - now;
                if (diff.TotalMinutes < 60)
                {
                    upcoming = $"{todayMeetings.Count} meeting(s) today (next in {(int)diff.TotalMinutes}m)";
                }
                else
                {
                    upcoming = $"{todayMeetings.Count} meeting(s) today";
                }
            }

            return new RoomDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                Location = room.Location,
                Capacity = room.Capacity,
                Equipment = room.Equipment,
                Status = statusStr,
                CreatedAt = room.CreatedAt,
                CurrentUser = currentUser,
                UpcomingSchedule = upcoming
            };
        }
    }
}
