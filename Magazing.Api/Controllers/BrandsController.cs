using Magazing.Application.DTOs.Catalog;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Brands.Commands;
using Magazing.Application.UseCases.Brands.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BrandsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.CatalogRead)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
    {
        return Ok(await sender.Send(new GetBrandsQuery(isActive)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await sender.Send(new GetBrandByIdQuery(id)));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> Create([FromBody] CreateBrandCommand request)
    {
        var brand = await sender.Send(request);
        return CreatedAtAction(nameof(GetById), new { id = brand.BrandId }, brand);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrandRequest request)
    {
        return Ok(await sender.Send(new UpdateBrandCommand(id, request.Name, request.IsActive)));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await sender.Send(new DeleteBrandCommand(id));
        return NoContent();
    }
}
