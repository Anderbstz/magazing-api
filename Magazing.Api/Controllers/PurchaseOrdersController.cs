using Magazing.Api.Authorization;
using Magazing.Application.DTOs.Purchases;
using Magazing.Application.Security;
using Magazing.Application.UseCases.PurchaseOrders.Commands;
using Magazing.Application.UseCases.PurchaseOrders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public class PurchaseOrdersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.PurchasesRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await sender.Send(new GetPurchaseOrdersQuery(status, supplierId, search, page, pageSize)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.PurchasesRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await sender.Send(new GetPurchaseOrderByIdQuery(id)));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PurchasesCreate)]
    public async Task<IActionResult> Create([FromBody] PurchaseOrderRequest request)
    {
        var order = await sender.Send(new CreatePurchaseOrderCommand(
            request.SupplierId,
            request.WarehouseId,
            request.ExpectedDate,
            request.Notes,
            request.Items,
            User.GetUserId()));

        return CreatedAtAction(nameof(GetById), new { id = order.PurchaseOrderId }, order);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.PurchasesCreate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PurchaseOrderRequest request)
    {
        return Ok(await sender.Send(new UpdatePurchaseOrderCommand(
            id,
            request.SupplierId,
            request.WarehouseId,
            request.ExpectedDate,
            request.Notes,
            request.Items)));
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = Permissions.PurchasesApprove)]
    public async Task<IActionResult> Approve(Guid id)
    {
        return Ok(await sender.Send(new ApprovePurchaseOrderCommand(id, User.GetUserId())));
    }

    [HttpPost("{id:guid}/receive")]
    [Authorize(Policy = Permissions.PurchasesReceive)]
    public async Task<IActionResult> Receive(Guid id)
    {
        return Ok(await sender.Send(new ReceivePurchaseOrderCommand(id, User.GetUserId())));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Permissions.PurchasesApprove)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        return Ok(await sender.Send(new CancelPurchaseOrderCommand(id)));
    }
}
