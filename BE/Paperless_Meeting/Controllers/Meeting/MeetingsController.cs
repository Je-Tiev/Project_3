using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Models;
using System.Security.Claims;
using Paperless_Meeting.Services.Meeting;
using Paperless_Meeting.Services.Participant;
using Paperless_Meeting.Repositories.Participant;

namespace Paperless_Meeting.Controllers.Meeting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;
        private readonly IParticipantService _participantService; // Thêm để lấy danh sách người tham gia => kiểm tra quyền vào meeting
        public MeetingsController(IMeetingService meetingService, IParticipantService participantService)
        {
            _meetingService = meetingService;
            _participantService = participantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMeetings()
        {
            var meetings = await _meetingService.GetAllMeetingsAsync();
            return Ok(meetings);
        }

        [HttpGet("user/my-meetings")]
        public async Task<IActionResult> GetMyMeetings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (userId == 0)
            {
                return Unauthorized(new { message = "Người dùng không hợp lệ." });
            }

            var meetings = await _meetingService.GetMeetingsByUserAsync(userId);
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
                RoomId = $"room-{Guid.NewGuid().ToString("N").Substring(0, 10)}",
                Description = requestDto.Description,
                StartTime = requestDto.StartTime,
                EndTime = requestDto.EndTime,
                Location = requestDto.Location
            };
            if (createDto.StartTime >= createDto.EndTime)
            {
                return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc." });
            }

            var creatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (creatorId ==0)
            {
                return Unauthorized(new { message = "Người dùng không hợp lệ." });
            }
    
            var createdMeeting = await _meetingService.CreateMeetingAsync(createDto, creatorId);
            
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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (userId == 0)
            {
                return Unauthorized(new { message = "Người dùng không hợp lệ." });
            }

            var success = await _meetingService.ExtendMeetingAsync(id, userId);
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
            //Lấy danh sách người tham gia
            var participants = await _participantService.GetMeetingParticipantsAsync(meetingId);
            //Kiểm tra quyền tham gia
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isParticipant = participants.Any(p => p.UserId == userId);
            if (!isParticipant)
            {
                return BadRequest("Bạn không có quyền tham gia cuộc họp này.");
            }
            //Kiểm tra thời gian tham gia
            var currentTime = DateTime.UtcNow;
            if (currentTime < meeting.StartTime || currentTime > meeting.EndTime)
            {
                return BadRequest("Hiện không phải thời gian diễn ra cuộc họp.");
            }
            var roomId = meeting.RoomId;
            return Ok(new
            {
                roomId,
                signalRUrl = "/meetinghub",
                meetingInfo = new
                {
                    meetingId,
                    title = meeting.Title,
                    startTime = meeting.StartTime.ToString("HH:mm dd/MM/yyyy"),
                    endTime = meeting.EndTime.ToString("HH:mm dd/MM/yyyy")
                },
                message = "Kết nối thành công."
            });
        }
    }
}