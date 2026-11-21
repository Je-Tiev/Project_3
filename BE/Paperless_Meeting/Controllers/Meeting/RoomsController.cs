using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Room;
using Paperless_Meeting.Repositories.Room;

namespace Paperless_Meeting.Controllers.Meeting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RoomDto>>> GetAllRooms()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoomById(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound("Không tìm thấy phòng họp.");
            }
            return Ok(room);
        }

        [HttpGet("check-availability")]
        public async Task<ActionResult<object>> CheckAvailability(
            [FromQuery] int roomId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] int? excludeMeetingId = null)
        {
            if (endTime <= startTime)
            {
                return BadRequest("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
            }

            var isAvailable = await _roomService.IsRoomAvailableAsync(roomId, startTime, endTime, excludeMeetingId);
            return Ok(new { roomId, isAvailable });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] RoomCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _roomService.CreateRoomAsync(createDto);
            return CreatedAtAction(nameof(GetRoomById), new { id = result.RoomId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] RoomCreateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _roomService.UpdateRoomAsync(id, updateDto);
            if (!success)
            {
                return NotFound("Không tìm thấy phòng họp cần cập nhật.");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var success = await _roomService.DeleteRoomAsync(id);
            if (!success)
            {
                return BadRequest("Không thể xóa phòng họp đang có cuộc họp diễn ra hoặc sắp tới.");
            }
            return NoContent();
        }
    }
}
