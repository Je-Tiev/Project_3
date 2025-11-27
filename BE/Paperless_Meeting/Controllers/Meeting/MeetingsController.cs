using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.DTOs.Meeting;
using System.Security.Claims;

namespace Paperless_Meeting.Controllers
{
    [ApiController]
    [Route("api/meeting")]
    [Authorize]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingService _meetingService;
        private readonly TimeZoneInfo _vnTimeZone;
        public MeetingController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
             _vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }

        //Xem tất cả cuộc họp
        [HttpGet]
        public async Task<IActionResult> GetAllMeetings()
        {
            var meetings = await _meetingService.GetAllMeetingsAsync();
            return Ok(meetings);
        }

        //Xem chi tiết cuộc họp theo ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMeetingById(int id)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(id);
            if (meeting == null) 
                return NotFound("Không tìmHB thấy cuộc họp.");
            var result = new
            {
                meeting.MeetingId,
                meeting.Title,
                meeting.Description,
                StartTime = TimeZoneInfo.ConvertTimeFromUtc(meeting.StartTime, _vnTimeZone),
                EndTime = TimeZoneInfo.ConvertTimeFromUtc(meeting.EndTime, _vnTimeZone),
                meeting.Location,
                meeting.Status,
                meeting.CreatedByUserId,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(meeting.CreatedAt, _vnTimeZone),
                meeting.Participants
            };
            return Ok(result);
        }

        //Tạo cuộc họp mới
        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] MeetingCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (createDto.Participants == null || !createDto.Participants.Any())
                return BadRequest(new { message = "Phải có ít nhất 1 người tham gia." });

            if (createDto.StartTime >= createDto.EndTime)
                return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc." });

            // Chuyển đổi múi giờ VN sang UTC
            createDto.StartTime = new DateTime(createDto.StartTime.Ticks, DateTimeKind.Unspecified);
            createDto.EndTime = new DateTime(createDto.EndTime.Ticks, DateTimeKind.Unspecified);
            createDto.StartTime = TimeZoneInfo.ConvertTimeToUtc(createDto.StartTime, _vnTimeZone);
            createDto.EndTime = TimeZoneInfo.ConvertTimeToUtc(createDto.EndTime, _vnTimeZone);


            var creatorClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(creatorClaim))
                return Unauthorized("Không xác thực người tạo.");

            var creatorId = int.Parse(creatorClaim);
            var created = await _meetingService.CreateMeetingAsync(createDto, creatorId);

            return CreatedAtAction(nameof(GetMeetingById), new { id = created.MeetingId }, created);
        }

        //Cập nhật cuộc họp
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMeeting(int id, [FromBody] MeetingUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (updateDto.StartTime >= updateDto.EndTime)
                return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc." });

            // Chuyển đổi múi giờ VN sang UTC
            updateDto.StartTime = new DateTime(updateDto.StartTime.Ticks, DateTimeKind.Unspecified);
            updateDto.EndTime = new DateTime(updateDto.EndTime.Ticks, DateTimeKind.Unspecified);
            updateDto.StartTime = TimeZoneInfo.ConvertTimeToUtc(updateDto.StartTime, _vnTimeZone);
            updateDto.EndTime = TimeZoneInfo.ConvertTimeToUtc(updateDto.EndTime, _vnTimeZone);

            var success = await _meetingService.UpdateMeetingAsync(id, updateDto);
            if (!success) 
                return NotFound("Không tìm thấy cuộc họp để cập nhật.");

            return NoContent();
        }

        //Xóa cuộc họp
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var success = await _meetingService.DeleteMeetingAsync(id);
            if (!success) 
                return NotFound("Không tìm thấy cuộc họp để xóa.");

            return NoContent();
        }

        // CHỨC NĂNG THAM GIA PHÒNG HỌP 
        // Tạo roomId ngẫu nhiên 
        [HttpPost("create-room")]
        public IActionResult CreateRoom()
        {
            var roomId = "room-" + Guid.NewGuid().ToString("N")[..10];
            return Ok(new { roomId });
        }


        // Kiểm tra quyền + trả về thông tin kết nối SignalR cho người dùng tham gia họp
        [HttpGet("join/{meetingId:int}")]
        public async Task<IActionResult> JoinRoom(int meetingId)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (meeting == null) 
                return NotFound("Không tìm thấy cuộc họp.");

            // Lấy FullName từ claim (AuthService đã add ClaimTypes.GivenName hoặc Name)
            var userFullName = User.FindFirstValue(ClaimTypes.GivenName) 
                            ?? User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(userFullName))
                return Forbid("Không xác thực tên người dùng.");

            // Kiểm tra user có trong danh sách participants không
            var isParticipant = meeting.Participants?.Any(p => 
                !string.IsNullOrWhiteSpace(p.FullName) &&
                string.Equals(p.FullName.Trim(), userFullName.Trim(), StringComparison.OrdinalIgnoreCase)) 
                ?? false;

            if (!isParticipant)
                return Forbid("Bạn không có quyền tham gia cuộc họp này.");

            // Chỉ cho phép tham gia trong khoảng thời gian cuộc họp
            var now = DateTime.UtcNow;
            if (now < meeting.StartTime)
                 return BadRequest(new { message = "Chưa đến thời gian bắt đầu cuộc họp." });

            if (now > meeting.EndTime)
                return BadRequest(new { message = "Cuộc họp đã kết thúc." });


            var roomId = $"meeting-{meetingId}";
            return Ok(new
            {
                roomId,
                signalRUrl = "/meetinghub",
                message = "Kết nối thành công."
            });
        }
    }
}