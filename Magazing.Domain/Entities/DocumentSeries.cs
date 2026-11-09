using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class DocumentSeries
{
    public Guid DocumentSeriesId { get; set; }

    public Guid BranchId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string Series { get; set; } = null!;

    public int CurrentNumber { get; set; }

    public bool IsActive { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
