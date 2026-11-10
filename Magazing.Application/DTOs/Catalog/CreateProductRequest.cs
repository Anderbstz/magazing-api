namespace Magazing.Application.DTOs.Catalog;

public record CreateProductRequest(
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
