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
        var sku = request.Sku?.Trim().ToUpper() ?? string.Empty;
        var barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim();
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
            throw new InvalidOperationException("Ya existe un producto con ese codigo de barras.");
        }

        var now = DateTime.UtcNow;
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Sku = sku,
            Barcode = barcode,
            Name = name,
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            UnitId = request.UnitId,
            IsTaxable = request.IsTaxable,
            MinStock = request.MinStock,
            MaxStock = request.MaxStock,
            AverageCost = 0,
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = true,
            CreatedBy = request.CreatedBy,
            CreatedAt = now
        };

        await products.AddAsync(product, cancellationToken);

        await unitOfWork.Repository<ProductPrice>().AddAsync(new ProductPrice
        {
            ProductPriceId = Guid.NewGuid(),
            ProductId = product.ProductId,
            Price = request.Price,
            ValidFrom = now,
            CreatedBy = request.CreatedBy,
            CreatedAt = now
        }, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return await ProductReadModel.GetAsync(unitOfWork, product.ProductId, cancellationToken);
    }
}
