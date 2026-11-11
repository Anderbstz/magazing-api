using Magazing.Api.Authorization;
using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Transfers.Commands;
using Magazing.Application.UseCases.Transfers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransfersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.InventoryRead)]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await sender.Send(new GetTransfersQuery(status, page, pageSize)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.InventoryRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await sender.Send(new GetTransferByIdQuery(id)));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.TransfersManage)]
    public async Task<IActionResult> Create([FromBody] CreateTransferRequest request)
    {
        var transfer = await sender.Send(new CreateTransferCommand(
            request.OriginWarehouseId,
            request.DestinationWarehouseId,
            request.Notes,
            request.Items,
            User.GetUserId()));

        return CreatedAtAction(nameof(GetById), new { id = transfer.TransferId }, transfer);
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = Permissions.TransfersManage)]
    public async Task<IActionResult> Complete(Guid id)
    {
        return Ok(await sender.Send(new CompleteTransferCommand(id, User.GetUserId())));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Permissions.TransfersManage)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        return Ok(await sender.Send(new CancelTransferCommand(id)));
    }
}
