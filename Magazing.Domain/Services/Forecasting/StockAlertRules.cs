namespace Magazing.Domain.Services.Forecasting;

public static class AlertTypes
{
    public const string LowStock = "LOW_STOCK";
    public const string PredictedShortage = "PREDICTED_SHORTAGE";
}

public static class AlertStatuses
{
    public const string Open = "OPEN";
    public const string Acknowledged = "ACKNOWLEDGED";
    public const string Resolved = "RESOLVED";

    public static bool IsValid(string? status)
    {
        return status is Open or Acknowledged or Resolved;
    }
}

public static class StockAlertRules
{
    public const decimal MinimumDemandForShortage = 1m;

    public static decimal? PredictedShortageOrderQuantity(decimal currentStock, decimal nextWeekDemand, decimal horizonDemand, decimal minStock)
    {
        if (nextWeekDemand < MinimumDemandForShortage || currentStock >= nextWeekDemand)
        {
            return null;
        }

        return Math.Max(1, Math.Ceiling(horizonDemand + minStock - currentStock));
    }

    public static decimal? LowStockOrderQuantity(decimal currentStock, decimal minStock, decimal? maxStock)
    {
        if (minStock <= 0 || currentStock > minStock)
        {
            return null;
        }

        return Math.Max(1, Math.Ceiling((maxStock ?? minStock * 2) - currentStock));
    }
}
