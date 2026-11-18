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

    Task<List<StockAlertDto>> GetAlertsAsync(string? status, List<Guid>? alertIds, CancellationToken cancellationToken = default);

    Task<List<string>> GetAlertRecipientEmailsAsync(string permissionCode, CancellationToken cancellationToken = default);
}
