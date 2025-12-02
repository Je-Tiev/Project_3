using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Participant;
using Paperless_Meeting.Repositories.Participant;
using Paperless_Meeting.Models;

namespace Paperless_Meeting.Services.Participant;

public class ParticipantService : IParticipantService
{
    private readonly PaperlessMeetingDbContext _context;

    public ParticipantService(PaperlessMeetingDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserViewDto>> GetMeetingParticipantsAsync(int meetingId)
    {
        var participants = await _context.MeetingParticipants
            .Where(mp => mp.MeetingId == meetingId)
            .Include(mp => mp.User)
            .Select(mp => new UserViewDto
            {
                UserId = mp.User.UserId,
                FullName = mp.User.FullName,
                Email = mp.User.Email
            })
            .ToListAsync();

        return participants;
    }

    public async Task<bool> AddParticipantAsync(int meetingId, int userId, string role)
    {
        // Kiểm tra meeting có tồn tại không
        var meetingExists = await _context.Meetings.AnyAsync(m => m.MeetingId == meetingId);
        if (!meetingExists)
            return false;

        // Kiểm tra user có tồn tại không
        var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
        if (!userExists)
            return false;

        // Kiểm tra participant đã tồn tại chưa
        var existingParticipant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(mp => mp.MeetingId == meetingId && mp.UserId == userId);

        if (existingParticipant != null)
            return false; // Participant đã tồn tại

        // Parse role
        if (!Enum.TryParse<MeetingParticipant.MeetingRole>(role, true, out var meetingRole))
            return false;

        // Thêm participant mới
        var newParticipant = new MeetingParticipant
        {
            MeetingId = meetingId,
            UserId = userId,
            RoleInMeeting = meetingRole,
            IsPresent = false
        };

        _context.MeetingParticipants.Add(newParticipant);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveParticipantAsync(int meetingId, int userId)
    {
        var participant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(mp => mp.MeetingId == meetingId && mp.UserId == userId);

        if (participant == null)
            return false;

        _context.MeetingParticipants.Remove(participant);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<AddParticipantResultDto>> AddMultipleParticipantsAsync(int meetingId, List<ParticipantDto> participants)
    {
        var results = new List<AddParticipantResultDto>();

        var meetingExists = await _context.Meetings.AnyAsync(m => m.MeetingId == meetingId);
        if (!meetingExists)
        {
            foreach (var participant in participants)
            {
                results.Add(new AddParticipantResultDto
                {
                    UserId = participant.UserId,
                    Success = false,
                    Message = "Meeting không tồn tại"
                });
            }
            return results;
        }

        var userIds = participants.Select(p => p.UserId).Distinct().ToList();

        var existingUsers = await _context.Users
            .Where(u => userIds.Contains(u.UserId))
            .Select(u => u.UserId)
            .ToListAsync();

        var existingParticipants = await _context.MeetingParticipants
            .Where(mp => mp.MeetingId == meetingId && userIds.Contains(mp.UserId))
            .Select(mp => mp.UserId)
            .ToListAsync();

        var participantsToAdd = new List<MeetingParticipant>();

        foreach (var participant in participants)
        {
            if (!existingUsers.Contains(participant.UserId))
            {
                results.Add(new AddParticipantResultDto
                {
                    UserId = participant.UserId,
                    Success = false,
                    Message = "User không tồn tại"
                });
                continue;
            }

            if (existingParticipants.Contains(participant.UserId))
            {
                results.Add(new AddParticipantResultDto
                {
                    UserId = participant.UserId,
                    Success = false,
                    Message = "Participant đã tồn tại trong meeting này"
                });
                continue;
            }

            if (!Enum.TryParse<MeetingParticipant.MeetingRole>(participant.RoleInMeeting.ToString(), true, out var meetingRole))
            {
                results.Add(new AddParticipantResultDto
                {
                    UserId = participant.UserId,
                    Success = false,
                    Message = "Role không hợp lệ"
                });
                continue;
            }

            participantsToAdd.Add(new MeetingParticipant
            {
                MeetingId = meetingId,
                UserId = participant.UserId,
                RoleInMeeting = meetingRole,
                IsPresent = false
            });

            results.Add(new AddParticipantResultDto
            {
                UserId = participant.UserId,
                Success = true,
                Message = "Thêm thành công"
            });
        }

        if (participantsToAdd.Any())
        {
            _context.MeetingParticipants.AddRange(participantsToAdd);
            await _context.SaveChangesAsync();
        }

        return results;
    }
    
    public async Task<bool> IsHostOfMeetingAsync(int meetingId, int userId)
    {
        return await _context.MeetingParticipants
            .AnyAsync(mp => mp.MeetingId == meetingId 
                            && mp.UserId == userId 
                            && mp.RoleInMeeting == MeetingParticipant.MeetingRole.Host);
    }
}