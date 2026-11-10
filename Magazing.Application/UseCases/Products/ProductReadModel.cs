using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Unit = Magazing.Domain.Entities.Unit;

namespace Magazing.Application.UseCases.Products;

internal static class ProductReadModel
{
    public static async Task<ProductDto> GetAsync(IUnitOfWork unitOfWork, Guid productId, CancellationToken cancellationToken)
    {
        var product = await unitOfWork.Repository<Product>().FirstOrDefaultAsync(item => item.ProductId == productId, cancellationToken)
            ?? throw new KeyNotFoundException("El producto no existe.");

        return (await ToDtosAsync(unitOfWork, [product], cancellationToken))[0];
    }

    public static async Task<List<ProductDto>> ToDtosAsync(IUnitOfWork unitOfWork, IReadOnlyList<Product> products, CancellationToken cancellationToken)
    {
        var productIds = products.Select(item => item.ProductId).ToList();
        var categoryIds = products.Select(item => item.CategoryId).Distinct().ToList();
        var brandIds = products.Where(item => item.BrandId != null).Select(item => item.BrandId!.Value).Distinct().ToList();
        var unitIds = products.Select(item => item.UnitId).Distinct().ToList();

        var categories = (await unitOfWork.Repository<Category>().FindAsync(item => categoryIds.Contains(item.CategoryId), cancellationToken))
            .ToDictionary(item => item.CategoryId, item => item.Name);

        var brands = (await unitOfWork.Repository<Brand>().FindAsync(item => brandIds.Contains(item.BrandId), cancellationToken))
            .ToDictionary(item => item.BrandId, item => item.Name);

        var units = (await unitOfWork.Repository<Unit>().FindAsync(item => unitIds.Contains(item.UnitId), cancellationToken))
            .ToDictionary(item => item.UnitId, item => item.Code);

        var prices = (await unitOfWork.Repository<ProductPrice>().FindAsync(
                item => productIds.Contains(item.ProductId) && item.ValidTo == null,
                cancellationToken))
            .GroupBy(item => item.ProductId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.ValidFrom).First().Price);

        return products.Select(product => new ProductDto
        {
            ProductId = product.ProductId,
            Sku = product.Sku,
            Barcode = product.Barcode,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = categories.GetValueOrDefault(product.CategoryId, string.Empty),
            BrandId = product.BrandId,
            BrandName = product.BrandId is { } brandId ? brands.GetValueOrDefault(brandId) : null,
            UnitId = product.UnitId,
            UnitCode = units.GetValueOrDefault(product.UnitId, string.Empty),
            IsTaxable = product.IsTaxable,
            MinStock = product.MinStock,
            MaxStock = product.MaxStock,
            AverageCost = product.AverageCost,
            CurrentPrice = prices.TryGetValue(product.ProductId, out var price) ? price : null,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        }).ToList();
    }

    public static async Task ValidateAsync(
        IUnitOfWork unitOfWork,
        Guid categoryId,
        Guid? brandId,
        Guid unitId,
        decimal minStock,
        decimal? maxStock,
        CancellationToken cancellationToken)
    {
        if (minStock < 0)
        {
            throw new InvalidOperationException("El stock minimo no puede ser negativo.");
        }

        if (maxStock is { } max && max < minStock)
        {
            throw new InvalidOperationException("El stock maximo no puede ser menor que el stock minimo.");
        }

        if (!await unitOfWork.Repository<Category>().AnyAsync(item => item.CategoryId == categoryId && item.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("La categoria no existe o esta inactiva.");
        }

        if (brandId is { } id && !await unitOfWork.Repository<Brand>().AnyAsync(item => item.BrandId == id && item.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("La marca no existe o esta inactiva.");
        }

        if (!await unitOfWork.Repository<Unit>().AnyAsync(item => item.UnitId == unitId, cancellationToken))
        {
            throw new InvalidOperationException("La unidad de medida no existe.");
        }
    }

    public static void ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new InvalidOperationException("El precio no puede ser negativo.");
        }

        if (decimal.Round(price, 2) != price)
        {
            throw new InvalidOperationException("El precio admite como maximo 2 decimales.");
        }
    }
}
