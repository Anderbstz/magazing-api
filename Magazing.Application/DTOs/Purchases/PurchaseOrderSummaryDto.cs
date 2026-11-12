namespace Magazing.Application.DTOs.Purchases;

public class PurchaseOrderSummaryDto
{
    public Guid PurchaseOrderId { get; set; }

    public string Code { get; set; } = null!;

    public Guid SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateOnly OrderDate { get; set; }

    public DateOnly? ExpectedDate { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }

    public int ItemCount { get; set; }

    public string CreatedByName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
