using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Products.Commands;

public record CreateProductCommand(
    string Sku,
    string? Barcode,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid? BrandId,
    Guid UnitId,
    bool IsTaxable,
    decimal MinStock,
    decimal? MaxStock,
    decimal Price,
    string? ImageUrl,
    Guid CreatedBy) : IRequest<ProductDto>;

internal sealed class CreateProductCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var sku = ProductFactory.NormalizeSku(request.Sku);
        var barcode = ProductFactory.NormalizeBarcode(request.Barcode);
        var name = request.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new InvalidOperationException("El SKU es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre del producto es obligatorio.");
        }

        ProductReadModel.ValidatePrice(request.Price);
        await ProductReadModel.ValidateAsync(unitOfWork, request.CategoryId, request.BrandId, request.UnitId, request.MinStock, request.MaxStock, cancellationToken);

        var products = unitOfWork.Repository<Product>();

        if (await products.AnyAsync(item => item.Sku == sku, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un producto con ese SKU.");
        }

        if (barcode is not null && await products.AnyAsync(item => item.Barcode == barcode, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un producto con ese código de barras.");
        }

        var product = await ProductFactory.AddAsync(unitOfWork, new NewProduct(
            sku,
            barcode,
            name,
            request.Description,
            request.CategoryId,
            request.BrandId,
            request.UnitId,
            request.IsTaxable,
            request.MinStock,
            request.MaxStock,
            request.Price,
            request.ImageUrl), request.CreatedBy, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return await ProductReadModel.GetAsync(unitOfWork, product.ProductId, cancellationToken);
    }
}
