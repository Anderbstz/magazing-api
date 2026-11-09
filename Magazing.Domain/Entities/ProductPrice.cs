using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class ProductPrice
{
    public Guid ProductPriceId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Price { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Product Product { get; set; } = null!;
}
