using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Company
{
    public Guid CompanyId { get; set; }

    public string Ruc { get; set; } = null!;

    public string BusinessName { get; set; } = null!;

    public string? TradeName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? LogoUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
