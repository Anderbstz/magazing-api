using Magazing.Api.Authorization;
using Magazing.Application.DTOs.Catalog;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Products.Commands;
using Magazing.Application.UseCases.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.CatalogRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] bool? isActive = null)
    {
        return Ok(await sender.Send(new GetProductsQuery(page, pageSize, search, categoryId, brandId, isActive)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await sender.Send(new GetProductByIdQuery(id)));
    }

    [HttpGet("{id:guid}/prices")]
    [Authorize(Policy = Permissions.CatalogRead)]
    public async Task<IActionResult> GetPriceHistory(Guid id)
    {
        return Ok(await sender.Send(new GetProductPriceHistoryQuery(id)));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var product = await sender.Send(new CreateProductCommand(
            request.Sku,
            request.Barcode,
            request.Name,
            request.Description,
            request.CategoryId,
            request.BrandId,
            request.UnitId,
            request.IsTaxable,
            request.MinStock,
            request.MaxStock,
            request.Price,
            request.ImageUrl,
            User.GetUserId()));

        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        return Ok(await sender.Send(new UpdateProductCommand(
            id,
            request.Barcode,
            request.Name,
            request.Description,
            request.CategoryId,
            request.BrandId,
            request.UnitId,
            request.IsTaxable,
            request.MinStock,
            request.MaxStock,
            request.ImageUrl,
            request.IsActive)));
    }

    [HttpPut("{id:guid}/price")]
    [Authorize(Policy = Permissions.PricesManage)]
    public async Task<IActionResult> ChangePrice(Guid id, [FromBody] ChangePriceRequest request)
    {
        return Ok(await sender.Send(new ChangeProductPriceCommand(id, request.Price, User.GetUserId())));
    }
}
