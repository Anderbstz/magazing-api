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
    [HttpGet("sales")]
    public async Task<IActionResult> Sales([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string? groupBy = null)
    {
        return Ok(await sender.Send(new GetSalesReportQuery(from, to, groupBy)));
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> TopProducts([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int limit = 10)
    {
        return Ok(await sender.Send(new GetTopProductsQuery(from, to, limit)));
    }
}
