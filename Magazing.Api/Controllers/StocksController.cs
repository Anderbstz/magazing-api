using Magazing.Application.Security;
using Magazing.Application.UseCases.Stocks.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.InventoryRead)]
public class StocksController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid warehouseId,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await sender.Send(new GetStocksQuery(warehouseId, status, search, page, pageSize)));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] Guid warehouseId)
    {
        return Ok(await sender.Send(new GetStockSummaryQuery(warehouseId)));
    }
}
