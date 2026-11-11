using Magazing.Application.DTOs.Purchases;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Suppliers.Commands;
using Magazing.Application.UseCases.Suppliers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.PurchasesRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        return Ok(await sender.Send(new GetSuppliersQuery(page, pageSize, search, isActive)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.PurchasesRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await sender.Send(new GetSupplierByIdQuery(id)));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.SuppliersManage)]
    public async Task<IActionResult> Create([FromBody] SupplierRequest request)
    {
        var supplier = await sender.Send(new CreateSupplierCommand(
            request.Ruc, request.BusinessName, request.ContactName, request.Email, request.Phone, request.Address));

        return CreatedAtAction(nameof(GetById), new { id = supplier.SupplierId }, supplier);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.SuppliersManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request)
    {
        return Ok(await sender.Send(new UpdateSupplierCommand(
            id, request.Ruc, request.BusinessName, request.ContactName, request.Email, request.Phone, request.Address, request.IsActive)));
    }
}
