using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class MovementType
{
    public Guid MovementTypeId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short Direction { get; set; }

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
