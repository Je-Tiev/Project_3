using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Paperless_Meeting.DTOs.Poll;
using Paperless_Meeting.Hubs;
using Paperless_Meeting.Repositories.Meeting;
using Paperless_Meeting.Repositories.Participant;
using Paperless_Meeting.Services.Poll; 
using System.Security.Claims;

namespace Paperless_Meeting.Controllers.Poll
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PollsController : ControllerBase
    {
        private readonly IPollService _pollService;
        private readonly IMeetingService _meetingService;
        private readonly IParticipantService _participantService;
        private readonly IHubContext<MeetingHub> _hubContext;
        public PollsController(
            IPollService pollService, 
            IMeetingService meetingService, 
            IParticipantService participantService,
            IHubContext<MeetingHub> hubContext)
        {
            _pollService = pollService;
            _meetingService = meetingService;
            _participantService = participantService;
            _hubContext = hubContext;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid User ID in Token");
        }

        // 1. TẠO POLL
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CreatePoll([FromBody] PollCreateRequestDto requestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Kiểm tra Meeting có tồn tại không
            var meeting = await _meetingService.GetMeetingByIdAsync(requestDto.MeetingId);
            if (meeting == null)
                return NotFound("Không tìm thấy cuộc họp.");

            // Kiểm tra thời gian cuộc họp 
            if (DateTime.UtcNow > meeting.EndTime)
                return BadRequest("Cuộc họp đã kết thúc, không thể tạo bình chọn.");

            var creatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var createdPoll = await _pollService.CreatePollAsync(requestDto, creatorId);

            //Broadcast cho mọi người
            if (!string.IsNullOrEmpty(meeting.RoomId))
            {
                await _hubContext.Clients.Group(meeting.RoomId).SendAsync("ReceiveNewPoll", createdPoll);
            }
            return CreatedAtAction(nameof(GetPollById), new { id = createdPoll.PollId }, createdPoll);
        }

        // 2. VOTE
        [HttpPost("{pollId}/vote")]
        public async Task<IActionResult> SubmitVote(int pollId, [FromBody] PollVoteRequestDto requestDto)
        {
            var userId = GetUserId();

            //Lấy thông tin Poll
            var poll = await _pollService.GetPollByIdAsync(pollId, userId);
            if (poll == null) return NotFound("Không tìm thấy cuộc bình chọn.");

            // Kiểm tra trạng thái Poll (đóng hoặc quá thời gian cuộc họp)
            if (poll.Status == Models.Poll.PollStatus.Closed.ToString())
                return BadRequest("Cuộc bình chọn đã đóng.");

            var meeting = await _meetingService.GetMeetingByIdAsync(poll.MeetingId);
            if (meeting == null) return NotFound("Không tìm thấy thông tin cuộc họp.");

            var currentTime = DateTime.UtcNow; 
            if (currentTime < meeting.StartTime)
                return BadRequest("Cuộc họp chưa bắt đầu, chưa thể bình chọn.");
            
            if (currentTime > meeting.EndTime)
                return BadRequest("Cuộc họp đã kết thúc, không thể bình chọn nữa.");

            var participants = await _participantService.GetMeetingParticipantsAsync(poll.MeetingId);
            var isParticipant = participants.Any(p => p.UserId == userId);
            
            if (!isParticipant) 
            {
                return BadRequest("Bạn không phải là thành viên của cuộc họp này.");
            }

            // Kiểm tra xem User đã vote chưa
            var hasVoted = await _pollService.HasUserVotedAsync(pollId, userId);
            if (hasVoted)
                return BadRequest("Bạn đã thực hiện bình chọn cho câu hỏi này rồi.");

            var success = await _pollService.SubmitVoteAsync(pollId, userId, requestDto.Choice);
            
            if (!success) return StatusCode(500, "Có lỗi xảy ra khi lưu phiếu bầu.");

            var updatedPollStats = await _pollService.GetPollByIdAsync(pollId, userId);
            if (!string.IsNullOrEmpty(meeting.RoomId))
            {
                await _hubContext.Clients.Group(meeting.RoomId).SendAsync("UpdatePollStats", updatedPollStats);
            }
            return Ok(new { message = "Bình chọn thành công.", choice = requestDto.Choice, data = updatedPollStats });
        }

        //lấy chi tiết Poll 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPollById(int id)
        {
            var userId = GetUserId();
            var poll = await _pollService.GetPollByIdAsync(id, userId);
            if (poll == null) return NotFound();
            return Ok(poll);
        }
        
        // Kết thúc bình chọn
        [HttpPut("{id}/close")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ClosePoll(int id)
        {
            var userId = GetUserId();
            var poll = await _pollService.GetPollByIdAsync(id, userId);
            if (poll == null) return NotFound();

            var success = await _pollService.ClosePollAsync(id);
            if (!success) return BadRequest("Không thể đóng cuộc bình chọn.");

            var meeting = await _meetingService.GetMeetingByIdAsync(poll.MeetingId);
            //Gửi tín hiệu đóng poll đến các user khác
            if (meeting != null && !string.IsNullOrEmpty(meeting.RoomId))
            {
                await _hubContext.Clients.Group(meeting.RoomId).SendAsync("PollClosed", new { pollId = id });
            }
            return Ok("Đã đóng bình chọn.");
        }
    }
}