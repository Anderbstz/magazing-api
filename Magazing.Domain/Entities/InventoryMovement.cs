using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class InventoryMovement
{
    public Guid InventoryMovementId { get; set; }

    public Guid ProductId { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid MovementTypeId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal BalanceAfter { get; set; }

    public decimal AverageCostAfter { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public string? Notes { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual MovementType MovementType { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
