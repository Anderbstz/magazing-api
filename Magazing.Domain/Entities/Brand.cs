using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Brand
{
    public Guid BrandId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
