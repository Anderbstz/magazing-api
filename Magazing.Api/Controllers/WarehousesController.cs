using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Warehouses.Commands;
using Magazing.Application.UseCases.Warehouses.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehousesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.InventoryRead)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
    {
        return Ok(await sender.Send(new GetWarehousesQuery(isActive)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.InventoryRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await sender.Send(new GetWarehouseByIdQuery(id)));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.WarehousesManage)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseCommand request)
    {
        var warehouse = await sender.Send(request);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.WarehouseId }, warehouse);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.WarehousesManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseRequest request)
    {
        return Ok(await sender.Send(new UpdateWarehouseCommand(id, request.Name, request.Address, request.IsActive)));
    }
}
