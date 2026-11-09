using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Stock
{
    public Guid StockId { get; set; }

    public Guid ProductId { get; set; }

    public Guid WarehouseId { get; set; }

    public decimal Quantity { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
