using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Transfer
{
    public Guid TransferId { get; set; }

    public string Code { get; set; } = null!;

    public Guid OriginWarehouseId { get; set; }

    public Guid DestinationWarehouseId { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public Guid? RequestedBy { get; set; }

    public Guid? CompletedBy { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? CompletedByNavigation { get; set; }

    public virtual Warehouse DestinationWarehouse { get; set; } = null!;

    public virtual Warehouse OriginWarehouse { get; set; } = null!;

    public virtual User? RequestedByNavigation { get; set; }

    public virtual ICollection<TransferDetail> TransferDetails { get; set; } = new List<TransferDetail>();
}
