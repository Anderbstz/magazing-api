using Magazing.Api.Configuration.Authorization;
using Magazing.Application.DTOs.Catalog;
using Magazing.Application.Services.Security;
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
    private const long MaxImportBytes = 5 * 1024 * 1024;

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

    [HttpGet("import/template")]
    [Authorize(Policy = Permissions.CatalogManage)]
    [Produces(FileContentTypes.Excel)]
    public async Task<IActionResult> ImportTemplate()
    {
        var file = await sender.Send(new GetProductImportTemplateQuery());
        return File(file.Content, FileContentTypes.Excel, file.FileName);
    }

    [HttpPost("import")]
    [Authorize(Policy = Permissions.CatalogManage)]
    [RequestSizeLimit(MaxImportBytes)]
    public async Task<IActionResult> Import(IFormFile? file, [FromQuery] bool dryRun = true)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("Sube un archivo .xlsx o .csv.");
        }

        await using var content = file.OpenReadStream();
        return Ok(await sender.Send(new ImportProductsCommand(content, file.FileName, dryRun, User.GetUserId())));
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
