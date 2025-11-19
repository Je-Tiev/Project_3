using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.Interfaces.Meeting;
using Paperless_Meeting.Services;
using System.Security.Claims;

namespace Paperless_Meeting.Controllers.Meeting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;
        private readonly IHmsService _hmsService;
        public MeetingsController(IMeetingService meetingService, IHmsService hmsService)
        {
            _meetingService = meetingService;
            _hmsService = hmsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMeetings()
        {
            var meetings = await _meetingService.GetAllMeetingsAsync();
            return Ok(meetings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeetingById(int id)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(id);
            if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");
            
            return Ok(meeting);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting(MeetingCreateDto createDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int creatorId))
            {
                return Unauthorized("Không thể xác định người dùng. Vui lòng đăng nhập lại");
            }
            
            var createdMeeting = await _meetingService.CreateMeetingAsync(createDto, creatorId);
            
            return CreatedAtAction(nameof(GetMeetingById), new { id = createdMeeting.MeetingId }, createdMeeting);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(int id, MeetingCreateDto updateDto)
        {
            var success = await _meetingService.UpdateMeetingAsync(id, updateDto);
            if (!success) return NotFound("Không tìm thấy cuộc họp để cập nhật.");
            
            return NoContent(); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var success = await _meetingService.DeleteMeetingAsync(id);
            if (!success) return NotFound("Không tìm thấy cuộc họp để xóa.");
            
            return NoContent();
        }
        [HttpPost("create-100ms")]
        public async Task<IActionResult> Create100msMeeting([FromBody] Create100msMeetingDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Tiêu đề cuộc họp không được để trống.");

            if (dto.Title.Length > 100)
                return BadRequest("Tiêu đề không được quá 100 ký tự.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Không xác định được người dùng.");

            var userIdFor100ms = int.TryParse(userIdClaim, out _) 
                ? userIdClaim 
                : User.FindFirst(ClaimTypes.Email)?.Value ?? $"user_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            try
            {
                var joinUrl = await _hmsService.CreateRoomAndGetJoinUrlAsync(
                    title: dto.Title.Trim(),
                    userId: userIdFor100ms,
                    role: "host" 
                );

                return Ok(new 
                { 
                    Message = "Tạo phòng họp thành công!",
                    JoinUrl = joinUrl,
                    RoomTitle = dto.Title
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Không thể tạo phòng họp. Vui lòng thử lại sau.");
            }
        }

        public record Create100msMeetingDto(string Title);
    }
}