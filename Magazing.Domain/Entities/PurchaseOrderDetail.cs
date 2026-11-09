using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class PurchaseOrderDetail
{
    public Guid PurchaseOrderDetailId { get; set; }

    public Guid PurchaseOrderId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}
