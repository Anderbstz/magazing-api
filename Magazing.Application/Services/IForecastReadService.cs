using Magazing.Application.DTOs.Forecasting;

namespace Magazing.Application.Services;

public interface IForecastReadService
{
    Task<List<WeeklySales>> GetWeeklySalesAsync(
        DateOnly fromWeekStart,
        DateOnly toWeekStartExclusive,
        Guid? productId,
        Guid? warehouseId,
        CancellationToken cancellationToken = default);
}
