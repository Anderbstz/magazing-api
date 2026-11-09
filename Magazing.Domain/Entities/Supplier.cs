using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Supplier
{
    public Guid SupplierId { get; set; }

    public string Ruc { get; set; } = null!;

    public string BusinessName { get; set; } = null!;

    public string? ContactName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
