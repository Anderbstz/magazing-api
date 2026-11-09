using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Warehouse
{
    public Guid WarehouseId { get; set; }

    public Guid BranchId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<DemandPrediction> DemandPredictions { get; set; } = new List<DemandPrediction>();

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<StockAlert> StockAlerts { get; set; } = new List<StockAlert>();

    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();

    public virtual ICollection<Transfer> TransferDestinationWarehouses { get; set; } = new List<Transfer>();

    public virtual ICollection<Transfer> TransferOriginWarehouses { get; set; } = new List<Transfer>();
}
