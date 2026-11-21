using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.Repositories.Report;

namespace Paperless_Meeting.Controllers.Meeting;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var report = await _reportService.GetOverviewReportAsync();
        return Ok(report);
    }
}
