using Paperless_Meeting.DTOs.Report;

namespace Paperless_Meeting.Repositories.Report;

public interface IReportService
{
    Task<MeetingReportOverviewDto> GetOverviewReportAsync();
}
