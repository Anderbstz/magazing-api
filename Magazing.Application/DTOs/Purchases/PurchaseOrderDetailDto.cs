namespace Magazing.Application.DTOs.Purchases;

public class PurchaseOrderDetailDto : PurchaseOrderSummaryDto
{
    public string SupplierRuc { get; set; } = null!;

    public string? Notes { get; set; }

    public string? ApprovedByName { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public List<PurchaseOrderItemDto> Items { get; set; } = [];
}

public class PurchaseOrderItemDto
{
    public Guid ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string UnitCode { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal Subtotal { get; set; }
}
