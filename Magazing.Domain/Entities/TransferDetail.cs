using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class TransferDetail
{
    public Guid TransferDetailId { get; set; }

    public Guid TransferId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Transfer Transfer { get; set; } = null!;
}
