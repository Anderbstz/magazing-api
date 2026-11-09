using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Unit
{
    public Guid UnitId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool AllowsDecimals { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
