using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;

namespace Magazing.Application.UseCases.Products;

internal sealed record NewProduct(
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
    string? ImageUrl);

internal static class ProductFactory
{
    public static string NormalizeSku(string? sku) => sku?.Trim().ToUpper() ?? string.Empty;

    public static string? NormalizeBarcode(string? barcode) => string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();

    public static async Task<Product> AddAsync(IUnitOfWork unitOfWork, NewProduct data, Guid createdBy, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Sku = NormalizeSku(data.Sku),
            Barcode = NormalizeBarcode(data.Barcode),
            Name = data.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(data.Description) ? null : data.Description.Trim(),
            CategoryId = data.CategoryId,
            BrandId = data.BrandId,
            UnitId = data.UnitId,
            IsTaxable = data.IsTaxable,
            MinStock = data.MinStock,
            MaxStock = data.MaxStock,
            AverageCost = 0,
            ImageUrl = data.ImageUrl?.Trim(),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = now
        };

        await unitOfWork.Repository<Product>().AddAsync(product, cancellationToken);

        await unitOfWork.Repository<ProductPrice>().AddAsync(new ProductPrice
        {
            ProductPriceId = Guid.NewGuid(),
            ProductId = product.ProductId,
            Price = data.Price,
            ValidFrom = now,
            CreatedBy = createdBy,
            CreatedAt = now
        }, cancellationToken);

        return product;
    }
}
