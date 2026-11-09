using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Branch
{
    public Guid BranchId { get; set; }

    public Guid CompanyId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<DocumentSeries> DocumentSeries { get; set; } = new List<DocumentSeries>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
