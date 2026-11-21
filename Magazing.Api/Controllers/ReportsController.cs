using Magazing.Application.Security;
using Magazing.Application.UseCases.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.ReportsRead)]
public class ReportsController(ISender sender) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        return Ok(await sender.Send(new GetDashboardQuery()));
    }

    [HttpGet("sales")]
    public async Task<IActionResult> Sales([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string? groupBy = null)
    {
        return Ok(await sender.Send(new GetSalesReportQuery(from, to, groupBy)));
    }

    [HttpGet("sales/export")]
    [Produces(FileContentTypes.Excel)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportSales([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string? groupBy = null)
    {
        var file = await sender.Send(new ExportSalesReportQuery(from, to, groupBy));
        return File(file.Content, FileContentTypes.Excel, file.FileName);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> TopProducts([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int limit = 10)
    {
        return Ok(await sender.Send(new GetTopProductsQuery(from, to, limit)));
    }
}
