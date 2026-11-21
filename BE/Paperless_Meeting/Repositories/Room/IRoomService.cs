using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Paperless_Meeting.DTOs.Room;

namespace Paperless_Meeting.Repositories.Room
{
    public interface IRoomService
    {
        Task<List<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto?> GetRoomByIdAsync(int id);
        Task<RoomDto> CreateRoomAsync(RoomCreateDto createDto);
        Task<bool> UpdateRoomAsync(int id, RoomCreateDto updateDto);
        Task<bool> DeleteRoomAsync(int id);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeMeetingId = null);
    }
}
