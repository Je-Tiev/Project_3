namespace Paperless_Meeting.DTOs.Report;

public class MeetingReportOverviewDto
{
    public int TotalMeetings { get; set; }
    public int TotalUsers { get; set; }
    public int TotalDocuments { get; set; }
    public int TotalRooms { get; set; }
    public int TotalPolls { get; set; }
    public int TotalVotes { get; set; }
    public double AttendanceRate { get; set; }
    public List<MonthlyCountDto> MonthlyMeetings { get; set; } = new();
    public List<RoomUtilizationDto> RoomUtilization { get; set; } = new();
    public List<DepartmentMeetingCountDto> DepartmentMeetings { get; set; } = new();
}

public class MonthlyCountDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RoomUtilizationDto
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int MeetingCount { get; set; }
    public double Percentage { get; set; }
}

public class DepartmentMeetingCountDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
}
