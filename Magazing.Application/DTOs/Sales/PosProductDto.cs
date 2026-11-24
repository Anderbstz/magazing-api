using Magazing.Application.DTOs.Catalog;

namespace Magazing.Application.DTOs.Sales;

public class PosProductDto
{
    public ProductDto Product { get; set; } = null!;

    public decimal QuantitySold { get; set; }

    public decimal Stock { get; set; }
}
