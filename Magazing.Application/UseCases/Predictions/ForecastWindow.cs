using Magazing.Application.Services.Common;
using Magazing.Application.DTOs.Forecasting;

namespace Magazing.Application.UseCases.Predictions;

internal static class ForecastWindow
{
    public const int DisplayedHistoryWeeks = 12;
    public const int HorizonWeeks = 4;

    public static DateOnly CurrentWeekStart()
    {
        return BusinessTime.WeekStart(BusinessTime.Today());
    }

    public static DateOnly HistoryStart(DateOnly currentWeekStart)
    {
        return currentWeekStart.AddDays(-7 * DisplayedHistoryWeeks);
    }

    public static List<(DateOnly WeekStart, decimal Quantity)> DenseHistory(
        IEnumerable<WeeklySales> sales,
        Guid productId,
        Guid warehouseId,
        DateOnly currentWeekStart)
    {
        var byWeek = sales
            .Where(item => item.ProductId == productId && item.WarehouseId == warehouseId)
            .ToDictionary(item => item.WeekStart, item => item.Quantity);

        return Enumerable.Range(0, DisplayedHistoryWeeks)
            .Select(index => HistoryStart(currentWeekStart).AddDays(7 * index))
            .Select(week => (week, byWeek.GetValueOrDefault(week)))
            .ToList();
    }
}
