using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class StockAlert
{
    public Guid StockAlertId { get; set; }

    public Guid ProductId { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid? DemandPredictionId { get; set; }

    public string AlertType { get; set; } = null!;

    public decimal CurrentStock { get; set; }

    public decimal Threshold { get; set; }

    public decimal? SuggestedOrderQty { get; set; }

    public string Status { get; set; } = null!;

    public Guid? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual DemandPrediction? DemandPrediction { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User? ResolvedByNavigation { get; set; }

    public virtual Warehouse Warehouse { get; set; } = null!;
}
