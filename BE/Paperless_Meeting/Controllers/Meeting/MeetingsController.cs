// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Paperless_Meeting.DTOs.Meeting;
// using Paperless_Meeting.Interfaces.Meeting;
// using Paperless_Meeting.Services;
// using System.Security.Claims;

// namespace Paperless_Meeting.Controllers.Meeting
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class MeetingsController : ControllerBase
//     {
//         private readonly IMeetingService _meetingService;
//         public MeetingsController(IMeetingService meetingService)
//         {
//             _meetingService = meetingService;
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetAllMeetings()
//         {
//             var meetings = await _meetingService.GetAllMeetingsAsync();
//             return Ok(meetings);
//         }

//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetMeetingById(int id)
//         {
//             var meeting = await _meetingService.GetMeetingByIdAsync(id);
//             if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

//             return Ok(meeting);
//         }

//         [HttpPost]
//         public async Task<IActionResult> CreateMeeting([FromBody] MeetingCreateDto createDto)
//         {
//             // Kiểm tra validation errors
//             if (!ModelState.IsValid)
//             {
//                 return BadRequest(ModelState);
//             }

//             // Kiểm tra participants không được rỗng
//             if (createDto.Participants == null || createDto.Participants.Count == 0)
//             {
//                 return BadRequest(new { message = "Phải có ít nhất 1 người tham gia." });
//             }

//             // Kiểm tra startTime < endTime
//             if (createDto.StartTime >= createDto.EndTime)
//             {
//                 return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc." });
//             }

//             var creatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

//             var createdMeeting = await _meetingService.CreateMeetingAsync(createDto, creatorId);

//             return CreatedAtAction(nameof(GetMeetingById), new { id = createdMeeting.MeetingId }, createdMeeting);
//         }


//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateMeeting(int id, MeetingCreateDto updateDto)
//         {
//             var success = await _meetingService.UpdateMeetingAsync(id, updateDto);
//             if (!success) return NotFound("Không tìm thấy cuộc họp để cập nhật.");

//             return NoContent(); 
//         }

//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteMeeting(int id)
//         {
//             var success = await _meetingService.DeleteMeetingAsync(id);
//             if (!success) return NotFound("Không tìm thấy cuộc họp để xóa.");

//             return NoContent();
//         }

//     }
// }
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.Interfaces.Meeting;
using System.Security.Claims;
using System.Linq;
using System;
using Paperless_Meeting.DTOs.Meeting;

namespace Paperless_Meeting.Controllers
{
    [ApiController]
    [Route("api/meeting")]
    [Authorize]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingService _meetingService;

        public MeetingController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        [HttpPost("create")]
        public IActionResult CreateRoom()
        {
            var roomId = "room-" + Guid.NewGuid().ToString("N")[..10];
            return Ok(new { roomId });
        }

        [HttpPost("create-meeting")]
        public async Task<IActionResult> CreateMeeting([FromBody] MeetingCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (createDto.Participants == null || createDto.Participants.Count == 0)
                return BadRequest(new { message = "Phải có ít nhất 1 người tham gia." });

            var creatorClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(creatorClaim))
                return Unauthorized("Không xác thực người tạo.");

            var creatorId = int.Parse(creatorClaim);
            var created = await _meetingService.CreateMeetingAsync(createDto, creatorId);

            return CreatedAtAction(nameof(JoinRoom), new { meetingId = created.MeetingId }, created);
        }

        [HttpGet("join/{meetingId}")]
        public async Task<IActionResult> JoinRoom(int meetingId)
        {
            // Lấy thông tin cuộc họp (bao gồm danh sách người tham gia)
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null) return NotFound("Không tìm thấy cuộc họp.");

            // Lấy tên đầy đủ của user từ claim (AuthService đã thêm ClaimTypes.GivenName)
            var userFullName = User.FindFirstValue(ClaimTypes.GivenName) ?? User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(userFullName))
                return Forbid("Không xác thực tên người dùng.");

            // Kiểm tra xem tên user có nằm trong danh sách participants không
            var allowed = meeting.Participants != null && meeting.Participants.Any(p =>
                !string.IsNullOrWhiteSpace(p.FullName) &&
                string.Equals(p.FullName.Trim(), userFullName.Trim(), System.StringComparison.OrdinalIgnoreCase)
            );

            if (!allowed)
            {
                return Forbid("Bạn không có quyền tham gia cuộc họp này.");
            }

            var roomId = $"meeting-{meetingId}";
            return Ok(new
            {
                roomId,
                signalRUrl = "/meetinghub",
                message = "Kết nối WebRTC thuần qua SignalR"
            });
        }
    }
}