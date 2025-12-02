using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Paperless_Meeting.DTOs.Participant;
using Paperless_Meeting.Models;
using Paperless_Meeting.Repositories.Participant;

namespace Paperless_Meeting.Controllers.Meeting;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ParticipantController : ControllerBase
{
    private readonly IParticipantService _participantService;

    public ParticipantController(IParticipantService participantService)
    {
        _participantService = participantService;
    }

    /// <summary>
    /// Lấy danh sách participants của một meeting
    /// </summary>
    /// <param name="meetingId">ID của meeting</param>
    /// <returns>Danh sách participants</returns>
    [HttpGet("meeting/{meetingId}")]
    public async Task<IActionResult> GetMeetingParticipants(int meetingId)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var allParticipants = await _participantService.GetMeetingParticipantsAsync(meetingId);
            var isParticipant = allParticipants.Any(p => p.UserId == userId);
    
            if (!isParticipant && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Unauthorized(new { message = "Chỉ người trong cuộc họp mới có quyền xem danh sách này" });
            }
            
            if (allParticipants == null || !allParticipants.Any())
            {
                return Ok(new List<UserViewDto>());
            }

            return Ok(allParticipants);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách participants", error = ex.Message });
        }
    }

    /// <summary>
    /// Thêm participant vào meeting
    /// </summary>
    /// <param name="meetingId">ID của meeting</param>
    /// <param name="participantDto">Thông tin participant</param>
    /// <returns>Kết quả thêm participant</returns>
    [HttpPost("meeting/{meetingId}")]
    public async Task<IActionResult> AddParticipant(int meetingId, [FromBody] ParticipantDto participantDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _participantService.AddParticipantAsync(
                meetingId, 
                participantDto.UserId, 
                participantDto.RoleInMeeting.ToString()
            );

            if (!result)
            {
                return BadRequest(new { message = "Không thể thêm participant. Có thể meeting/user không tồn tại hoặc participant đã tồn tại." });
            }

            return Ok(new { message = "Thêm participant thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Có lỗi xảy ra khi thêm participant", error = ex.Message });
        }
    }

    /// <summary>
    /// Thêm nhiều participants vào meeting cùng lúc
    /// </summary>
    /// <param name="meetingId">ID của meeting</param>
    /// <param name="addMultipleDto">Danh sách participants cần thêm</param>
    /// <returns>Kết quả thêm từng participant (kể cả thành công hay thất bại)</returns>
    [HttpPost("meeting/{meetingId}/bulk")]
    public async Task<IActionResult> AddMultipleParticipants(int meetingId, [FromBody] AddMultipleParticipantsDto addMultipleDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isHost = await _participantService.IsHostOfMeetingAsync(meetingId, userId);

            if (!isHost)
            {
                return Unauthorized(new { message = "Chỉ Host mới có quyền thêm người tham gia" });
            }
            var results = await _participantService.AddMultipleParticipantsAsync(meetingId, addMultipleDto.Participants);

            if (results == null || !results.Any())
            {
                return BadRequest(new { message = "Không có participants để thêm" });
            }

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            return Ok(new
            {
                message = $"Hoàn tất: {successCount} thành công, {failureCount} thất bại",
                successCount,
                failureCount,
                details = results
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Có lỗi xảy ra khi thêm participants", error = ex.Message });
        }
    }

    /// <summary>
    /// Xóa participant khỏi meeting
    /// </summary>
    /// <param name="meetingId">ID của meeting</param>
    /// <param name="userId">ID của user</param>
    /// <returns>Kết quả xóa participant</returns>
    [HttpDelete("meeting/{meetingId}/user/{userId}")]
    public async Task<IActionResult> RemoveParticipant(int meetingId, int userId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Chỉ Host mới có quyền xóa người tham gia 
            if (userId != currentUserId)
            {
                var isHost = await _participantService.IsHostOfMeetingAsync(meetingId, currentUserId);

                if (!isHost)
                {
                    return Unauthorized(new { message = "Chỉ Host mới có quyền xóa người tham gia" });
                }
            }
            
            var result = await _participantService.RemoveParticipantAsync(meetingId, userId);

            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy participant để xóa" });
            }

            return Ok(new { message = "Xóa participant thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Có lỗi xảy ra khi xóa participant", error = ex.Message });
        }
    }
}