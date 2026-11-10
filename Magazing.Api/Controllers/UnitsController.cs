using Magazing.Application.DTOs.Catalog;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Units.Commands;
using Magazing.Application.UseCases.Units.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.CatalogRead)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await sender.Send(new GetUnitsQuery()));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> Create([FromBody] CreateUnitCommand request)
    {
        var unit = await sender.Send(request);
        return CreatedAtAction(nameof(GetAll), null, unit);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitRequest request)
    {
        return Ok(await sender.Send(new UpdateUnitCommand(id, request.Name, request.AllowsDecimals)));
    }
}
