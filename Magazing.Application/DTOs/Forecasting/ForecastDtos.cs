namespace Magazing.Application.DTOs.Forecasting;

public record WeeklySales(Guid ProductId, Guid WarehouseId, DateOnly WeekStart, decimal Quantity);

public class DemandForecastDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string UnitCode { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public string ModelVersion { get; set; } = null!;

    public decimal CurrentStock { get; set; }

    public List<ForecastPointDto> Points { get; set; } = [];
}

public class ForecastPointDto
{
    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public decimal? ActualQuantity { get; set; }

    public decimal? PredictedQuantity { get; set; }

    public decimal? LowerBound { get; set; }

    public decimal? UpperBound { get; set; }
}

public class RunPredictionsResultDto
{
    public int Created { get; set; }

    public string ModelVersion { get; set; } = null!;

    public int ActualsUpdated { get; set; }

    public int AlertsCreated { get; set; }
}
