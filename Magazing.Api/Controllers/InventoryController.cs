using Magazing.Api.Authorization;
using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Inventory.Commands;
using Magazing.Application.UseCases.Inventory.Queries;
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
