using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Customer
{
    public Guid CustomerId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string DocumentNumber { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
