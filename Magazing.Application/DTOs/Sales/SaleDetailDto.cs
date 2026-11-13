namespace Magazing.Application.DTOs.Sales;

public class SaleDetailDto : SaleSummaryDto
{
    public Guid WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal DiscountTotal { get; set; }

    public decimal TaxAmount { get; set; }

    public string? Notes { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancelReason { get; set; }

    public decimal Change { get; set; }

    public List<SaleItemDto> Items { get; set; } = [];

    public List<SalePaymentDto> Payments { get; set; } = [];
}

public class SaleItemDto
{
    public Guid ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string UnitCode { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal LineTotal { get; set; }
}

public class SalePaymentDto
{
    public Guid PaymentMethodId { get; set; }

    public string MethodName { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Reference { get; set; }

    public DateTime PaidAt { get; set; }
}
