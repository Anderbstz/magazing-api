using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class SaleDetail
{
    public Guid SaleDetailId { get; set; }

    public Guid SaleId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal UnitCost { get; set; }

    public decimal LineTotal { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;
}
