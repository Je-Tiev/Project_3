using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.Repositories.Meeting;
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
        private readonly TimeZoneInfo _vnTimeZone;

        public MeetingsController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
            _vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Vietnam timezone
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
            if (meeting == null) 
                return NotFound(new { message = "Không tìm thấy cuộc họp." });
            
            return Ok(meeting);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] MeetingCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { message = "Dữ liệu không hợp lệ.", errors });
            }

            if (createDto.StartTime >= createDto.EndTime)
            {
                return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc." });
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int creatorId))
            {
                return Unauthorized(new { message = "Không thể xác thực người tạo cuộc họp." });
            }

            try
            {
                var roomId = $"room-{Guid.NewGuid():N}[0..10]";
                var createdMeeting = await _meetingService.CreateMeetingAsync(createDto, creatorId, roomId);
                return CreatedAtAction(nameof(GetMeetingById), 
                    new { id = createdMeeting.MeetingId }, 
                    new { message = "Tạo cuộc họp thành công.", data = createdMeeting, roomId = createdMeeting.RoomId });
            }
            catch (Exception ex)
            {
                // Log exception here
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo cuộc họp." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(int id, [FromBody] MeetingUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { message = "Dữ liệu không hợp lệ.", errors });
            }

            if (updateDto.StartTime >= updateDto.EndTime)
            {
                return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc." });
            }

            try
            {
                var success = await _meetingService.UpdateMeetingAsync(id, updateDto);
                if (!success) 
                    return NotFound(new { message = "Không tìm thấy cuộc họp để cập nhật." });
                
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log exception here
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật cuộc họp." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            try
            {
                var success = await _meetingService.DeleteMeetingAsync(id);
                if (!success) 
                    return NotFound(new { message = "Không tìm thấy cuộc họp để xóa." });
                
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log exception here
                return StatusCode(500, new { message = "Có lỗi xảy ra khi xóa cuộc họp." });
            }
        }

        [HttpGet("join/{meetingId:int}")]
        public async Task<IActionResult> JoinRoom(int meetingId)
        {
            // Validate meeting exists
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                return NotFound(new { message = "Không tìm thấy cuộc họp." });

            // Validate user identity
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Không thể xác thực người dùng." });

            var isParticipant = meeting.Participants?.Any(p => p.UserId == userId) == true;
            
            if (!isParticipant)
                return BadRequest(new { message = "Bạn không có quyền tham gia cuộc họp này." });

            // Check meeting time (Vietnam timezone)
            var nowUtc = DateTime.UtcNow;
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _vnTimeZone);
            var startTime = (DateTime)meeting.StartTime!;
            var endTime = (DateTime)meeting.EndTime!;

            if (nowVn < startTime)
                return BadRequest(new { message = $"Cuộc họp chưa bắt đầu. Thời gian bắt đầu: {startTime:HH:mm dd/MM/yyyy}" });

            if (nowVn > endTime)
                return BadRequest(new { message = $"Cuộc họp đã kết thúc. Thời gian kết thúc: {endTime:HH:mm dd/MM/yyyy}" });

            var roomId = $"meeting-{meetingId}";
            return Ok(new
            {
                roomId,
                signalRUrl = "/meetinghub",
                meetingInfo = new
                {
                    meetingId,
                    title = meeting.Title,
                    startTime = startTime.ToString("HH:mm dd/MM/yyyy"),
                    endTime = endTime.ToString("HH:mm dd/MM/yyyy")
                },
                message = "Kết nối thành công."
            });
        }
    }
}