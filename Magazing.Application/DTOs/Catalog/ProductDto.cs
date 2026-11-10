namespace Magazing.Application.DTOs.Catalog;

public class ProductDto
{
    public Guid ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string? Barcode { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public Guid? BrandId { get; set; }

    public string? BrandName { get; set; }

    public Guid UnitId { get; set; }

    public string UnitCode { get; set; } = null!;

    public bool IsTaxable { get; set; }

    public decimal MinStock { get; set; }

    public decimal? MaxStock { get; set; }

    public decimal AverageCost { get; set; }

    public decimal? CurrentPrice { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
