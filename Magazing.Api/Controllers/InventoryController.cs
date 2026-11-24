using Magazing.Api.Configuration.Authorization;
using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Services.Security;
using Magazing.Application.UseCases.Inventory.Commands;
using Magazing.Application.UseCases.Inventory.Queries;
using Magazing.Application.UseCases.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController(ISender sender) : ControllerBase
{
    [HttpGet("movements")]
    [Authorize(Policy = Permissions.InventoryRead)]
    public async Task<IActionResult> GetMovements(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] string? type = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await sender.Send(new GetMovementsQuery(productId, warehouseId, type, from, to, page, pageSize)));
    }

    [HttpGet("movements/export")]
    [Authorize(Policy = Permissions.InventoryRead)]
    [Produces(FileContentTypes.Excel)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportMovements(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] string? type = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null)
    {
        var file = await sender.Send(new ExportKardexQuery(productId, warehouseId, type, from, to));
        return File(file.Content, FileContentTypes.Excel, file.FileName);
    }

    [HttpPost("adjustments")]
    [Authorize(Policy = Permissions.InventoryAdjust)]
    public async Task<IActionResult> CreateAdjustment([FromBody] AdjustmentRequest request)
    {
        var movement = await sender.Send(new CreateAdjustmentCommand(
            request.ProductId,
            request.WarehouseId,
            request.Type,
            request.Quantity,
            request.UnitCost,
            request.Reason,
            User.GetUserId()));

        return StatusCode(StatusCodes.Status201Created, movement);
    }
}
