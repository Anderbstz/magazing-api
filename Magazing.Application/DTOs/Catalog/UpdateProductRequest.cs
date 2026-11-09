namespace Magazing.Application.DTOs.Catalog;

public record UpdateProductRequest(
    string? Barcode,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid? BrandId,
    Guid UnitId,
    bool IsTaxable,
    decimal MinStock,
    decimal? MaxStock,
    string? ImageUrl,
    bool IsActive);
