using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Sale
{
    public Guid SaleId { get; set; }

    public Guid BranchId { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid SellerId { get; set; }

    public string Status { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal DiscountTotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }

    public string? Notes { get; set; }

    public DateTime SoldAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CancelReason { get; set; }

    public Guid? CancelledBy { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual User? CancelledByNavigation { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Invoice? Invoice { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    public virtual User Seller { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
