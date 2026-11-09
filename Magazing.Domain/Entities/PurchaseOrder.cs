using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class PurchaseOrder
{
    public Guid PurchaseOrderId { get; set; }

    public string Code { get; set; } = null!;

    public Guid SupplierId { get; set; }

    public Guid WarehouseId { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly OrderDate { get; set; }

    public DateOnly? ExpectedDate { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }

    public string? Notes { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
