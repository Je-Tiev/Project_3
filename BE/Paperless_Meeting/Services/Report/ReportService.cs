using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Report;
using Paperless_Meeting.Repositories.Report;

namespace Paperless_Meeting.Services.Report;

public class ReportService : IReportService
{
    private readonly PaperlessMeetingDbContext _context;

    public ReportService(PaperlessMeetingDbContext context)
    {
        _context = context;
    }

    public async Task<MeetingReportOverviewDto> GetOverviewReportAsync()
    {
        var totalMeetings = await _context.Meetings.CountAsync();
        var totalUsers = await _context.Users.CountAsync();
        var totalDocuments = await _context.Documents.CountAsync();
        var totalRooms = await _context.Rooms.CountAsync();
        var totalPolls = await _context.Polls.CountAsync();
        var totalVotes = await _context.UserVotes.CountAsync();

        // Calculate attendance rate: total participants / (total meetings * average participants or base)
        var totalParticipations = await _context.MeetingParticipants.CountAsync();
        double attendanceRate = 85.0; // default healthy baseline
        if (totalMeetings > 0 && totalUsers > 0)
        {
            var possibleInvites = totalMeetings * Math.Min(totalUsers, 5);
            if (possibleInvites > 0)
            {
                attendanceRate = Math.Min(100.0, Math.Round(((double)totalParticipations / possibleInvites) * 100, 1));
            }
        }

        // Monthly meetings (last 7 months)
        var now = DateTime.UtcNow;
        var monthlyList = new List<MonthlyCountDto>();
        for (int i = 6; i >= 0; i--)
        {
            var targetMonth = now.AddMonths(-i);
            var year = targetMonth.Year;
            var month = targetMonth.Month;
            var monthName = targetMonth.ToString("MMM");

            var count = await _context.Meetings
                .CountAsync(m => m.StartTime.Year == year && m.StartTime.Month == month);

            monthlyList.Add(new MonthlyCountDto
            {
                Month = monthName,
                Count = count
            });
        }

        // Room utilization
        var rooms = await _context.Rooms.AsNoTracking().ToListAsync();
        var roomStats = new List<RoomUtilizationDto>();
        foreach (var r in rooms)
        {
            var meetingCount = await _context.Meetings.CountAsync(m => m.PhysicalRoomId == r.RoomId);
            double pct = totalMeetings > 0 ? Math.Round(((double)meetingCount / totalMeetings) * 100, 1) : 0;
            roomStats.Add(new RoomUtilizationDto
            {
                RoomId = r.RoomId,
                RoomName = r.RoomName,
                MeetingCount = meetingCount,
                Percentage = pct
            });
        }

        // Department breakdown
        var deptStats = await _context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentMeetingCountDto
            {
                DepartmentName = d.DepartmentName,
                MemberCount = d.Members.Count()
            })
            .ToListAsync();

        return new MeetingReportOverviewDto
        {
            TotalMeetings = totalMeetings,
            TotalUsers = totalUsers,
            TotalDocuments = totalDocuments,
            TotalRooms = totalRooms,
            TotalPolls = totalPolls,
            TotalVotes = totalVotes,
            AttendanceRate = attendanceRate,
            MonthlyMeetings = monthlyList,
            RoomUtilization = roomStats,
            DepartmentMeetings = deptStats
        };
    }
}
