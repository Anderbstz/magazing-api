using Magazing.Application.Common;
using Magazing.Application.DTOs.Forecasting;
using Magazing.Application.Services;
using Magazing.Domain.Sales;
using Magazing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Magazing.Infrastructure.Adapters.Services;

public class ForecastReadService(MagazingDbContext context) : IForecastReadService
{
    public async Task<List<WeeklySales>> GetWeeklySalesAsync(
        DateOnly fromWeekStart,
        DateOnly toWeekStartExclusive,
        Guid? productId,
        Guid? warehouseId,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = BusinessTime.StartOfDayUtc(fromWeekStart);
        var toUtc = BusinessTime.StartOfDayUtc(toWeekStartExclusive);

        var query = context.SaleDetails.AsNoTracking()
            .Where(item => item.Sale.Status == SaleStatuses.Completed
                && item.Sale.SoldAt >= fromUtc
                && item.Sale.SoldAt < toUtc);

        if (productId is { } product)
        {
            query = query.Where(item => item.ProductId == product);
        }

        if (warehouseId is { } warehouse)
        {
            query = query.Where(item => item.Sale.WarehouseId == warehouse);
        }

        var rows = await query
            .Select(item => new { item.ProductId, item.Sale.WarehouseId, item.Sale.SoldAt, item.Quantity })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(item => new { item.ProductId, item.WarehouseId, Week = BusinessTime.WeekStartOf(item.SoldAt) })
            .Select(group => new WeeklySales(group.Key.ProductId, group.Key.WarehouseId, group.Key.Week, group.Sum(item => item.Quantity)))
            .ToList();
    }
}
