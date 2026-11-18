namespace Magazing.Application.DTOs.Forecasting;

public class StockAlertDto
{
    public Guid StockAlertId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public string UnitCode { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public string AlertType { get; set; } = null!;

    public decimal CurrentStock { get; set; }

    public decimal Threshold { get; set; }

    public decimal? SuggestedOrderQty { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? ResolvedByName { get; set; }
}
