using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class DemandPrediction
{
    public Guid DemandPredictionId { get; set; }

    public Guid ProductId { get; set; }

    public Guid WarehouseId { get; set; }

    public string ModelVersion { get; set; } = null!;

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public decimal PredictedQuantity { get; set; }

    public decimal? LowerBound { get; set; }

    public decimal? UpperBound { get; set; }

    public decimal? ActualQuantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<StockAlert> StockAlerts { get; set; } = new List<StockAlert>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
