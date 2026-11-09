using Magazing.Application.DTOs.Catalog;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Categories.Commands;
using Magazing.Application.UseCases.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.CatalogRead)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
    {
        return Ok(await sender.Send(new GetCategoriesQuery(isActive)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await sender.Send(new GetCategoryByIdQuery(id)));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand request)
    {
        var category = await sender.Send(request);
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        return Ok(await sender.Send(new UpdateCategoryCommand(id, request.Name, request.Description, request.ParentCategoryId, request.IsActive)));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await sender.Send(new DeleteCategoryCommand(id));
        return NoContent();
    }
}
