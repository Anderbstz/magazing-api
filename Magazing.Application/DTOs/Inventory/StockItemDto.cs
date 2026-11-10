namespace Magazing.Application.DTOs.Inventory;

public class StockItemDto
{
    public Guid ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string UnitCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public decimal Quantity { get; set; }

    public decimal MinStock { get; set; }

    public decimal? MaxStock { get; set; }

    public decimal AverageCost { get; set; }

    public decimal StockValue { get; set; }

    public string Status { get; set; } = null!;
}
