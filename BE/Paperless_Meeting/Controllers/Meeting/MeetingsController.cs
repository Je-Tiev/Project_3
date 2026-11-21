using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.Participant;
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
        private readonly IParticipantService _participantService;

        public MeetingsController(IMeetingService meetingService, IParticipantService participantService)
        {
            _meetingService = meetingService;
            _participantService = participantService;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) && id > 0 ? id : null;
        }

        private bool IsCurrentUserAdmin()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Lấy tất cả cuộc họp (Chỉ Admin / SuperAdmin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllMeetings()
        {
            var meetings = await _meetingService.GetAllMeetingsAsync();
            return Ok(meetings);
        }

        /// <summary>
        /// Lấy danh sách cuộc họp của người dùng hiện tại
        /// </summary>
        [HttpGet("user/my-meetings")]
        [HttpGet("my-meetings")]
        public async Task<IActionResult> GetMyMeetings()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Không thể xác định danh tính người dùng." });
            }

            var meetings = await _meetingService.GetMeetingsByUserAsync(userId.Value);
            return Ok(meetings);
        }

        /// <summary>
        /// Lấy thông tin chi tiết cuộc họp theo ID (Kiểm tra quyền thành viên)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeetingById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Không thể xác định danh tính người dùng." });
            }

            if (!IsCurrentUserAdmin())
            {
                var isParticipant = await _participantService.IsParticipantAsync(id, userId.Value);
                if (!isParticipant)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền xem thông tin cuộc họp này." });
                }
            }

            var meeting = await _meetingService.GetMeetingByIdAsync(id);
            if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");
            
            return Ok(meeting);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CreateMeeting([FromBody] MeetingCreateRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { message = "Dữ liệu không hợp lệ.", errors });
            }
            
            var createDto = new MeetingCreateDto
            {
                Title = requestDto.Title,
                Description = requestDto.Description,
                StartTime = requestDto.StartTime,
                EndTime = requestDto.EndTime,
                RoomId = requestDto.RoomId
            };

            if (createDto.StartTime >= createDto.EndTime)
            {
                return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc." });
            }

            var creatorId = GetCurrentUserId();
            if (creatorId == null)
            {
                return Unauthorized(new { message = "Không thể xác định danh tính người dùng." });
            }
    
            var createdMeeting = await _meetingService.CreateMeetingAsync(createDto, creatorId.Value);
            return CreatedAtAction(nameof(GetMeetingById), new { id = createdMeeting.MeetingId }, createdMeeting);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateMeeting(int id, [FromBody] MeetingUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (updateDto.StartTime >= updateDto.EndTime)
            {
                return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc." });
            }

            var success = await _meetingService.UpdateMeetingAsync(id, updateDto);
            if (!success) return NotFound("Không tìm thấy cuộc họp để cập nhật.");
            
            return NoContent(); 
        }

        [HttpPost("{id}/extend")]
        [Authorize]
        public async Task<IActionResult> ExtendMeeting(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Không thể xác định danh tính người dùng." });
            }

            var success = await _meetingService.ExtendMeetingAsync(id, userId.Value);
            if (!success) 
                return BadRequest(new { message = "Không thể gia hạn cuộc họp. Chỉ Host của cuộc họp mới có quyền gia hạn hoặc cuộc họp không tồn tại." });
            
            return Ok(new { message = "Gia hạn cuộc họp thêm 1 tiếng thành công." });
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var success = await _meetingService.DeleteMeetingAsync(id);
            if (!success) return NotFound("Không tìm thấy cuộc họp để xóa.");
            
            return NoContent();
        }

        [HttpGet("join/{meetingId}")]
        public async Task<IActionResult> JoinMeeting(int meetingId)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Không thể xác định danh tính người dùng." });
            }

            // Admin hoặc người tham gia đều được phép vào
            if (!IsCurrentUserAdmin())
            {
                var isParticipant = await _participantService.IsParticipantAsync(meetingId, userId.Value);
                if (!isParticipant)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền tham gia cuộc họp này." });
                }
            }

            var currentTime = DateTime.UtcNow;
            if (currentTime < meeting.StartTime || currentTime > meeting.EndTime)
            {
                return BadRequest("Hiện không phải thời gian diễn ra cuộc họp.");
            }

            var roomId = meeting.RoomId;
            return Ok(new
            {
                roomId,
                meetingId = meeting.MeetingId,
                title = meeting.Title,
                startTime = meeting.StartTime,
                endTime = meeting.EndTime,
                message = "Bạn có quyền tham gia cuộc họp này."
            });
        }
    }
}
