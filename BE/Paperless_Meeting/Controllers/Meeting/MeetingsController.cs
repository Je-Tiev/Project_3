using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.Interfaces.Meeting;
using Paperless_Meeting.Models;
using System.Security.Claims;

namespace Paperless_Meeting.Controllers.Meeting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;

        public MeetingsController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
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
        public async Task<IActionResult> CreateMeeting([FromBody] MeetingCreateDto createDto)
        {
            // Kiểm tra validation errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra participants không được rỗng
            if (createDto.Participants == null || createDto.Participants.Count == 0)
            {
                return BadRequest(new { message = "Phải có ít nhất 1 người tham gia." });
            }

            // Kiểm tra startTime < endTime
            if (createDto.StartTime >= createDto.EndTime)
            {
                return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc." });
            }

            var creatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var createdMeeting = await _meetingService.CreateMeetingAsync(createDto, creatorId);
            
            return CreatedAtAction(nameof(GetMeetingById), new { id = createdMeeting.MeetingId }, createdMeeting);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(int id, [FromBody] MeetingCreateDto updateDto)
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
    }
}