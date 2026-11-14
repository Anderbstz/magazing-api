using Magazing.Api.Authorization;
using Magazing.Application.DTOs.Sales;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Sales.Commands;
using Magazing.Application.UseCases.Sales.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.SalesRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] string? documentType = null,
        [FromQuery] string? search = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await sender.Send(new GetSalesQuery(status, documentType, search, from, to, page, pageSize)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.SalesRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await sender.Send(new GetSaleByIdQuery(id)));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.SalesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request)
    {
        var sale = await sender.Send(new CreateSaleCommand(
            request.WarehouseId,
            request.CustomerId,
            request.DocumentType,
            request.Notes,
            request.Items,
            request.Payments,
            User.GetUserId()));

        return CreatedAtAction(nameof(GetById), new { id = sale.SaleId }, sale);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Permissions.SalesCancel)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelSaleRequest request)
    {
        return Ok(await sender.Send(new CancelSaleCommand(id, request.Reason, User.GetUserId())));
    }
}
