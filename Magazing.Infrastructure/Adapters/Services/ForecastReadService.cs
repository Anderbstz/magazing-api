using Magazing.Application.Services.Common;
using Magazing.Application.DTOs.Forecasting;
using Magazing.Application.Services;
using Magazing.Domain.Services.Sales;
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

    public async Task<List<StockAlertDto>> GetAlertsAsync(string? status, List<Guid>? alertIds, CancellationToken cancellationToken = default)
    {
        var query = context.StockAlerts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(item => item.Status == status);
        }

        if (alertIds is not null)
        {
            query = query.Where(item => alertIds.Contains(item.StockAlertId));
        }

        return await query
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new StockAlertDto
            {
                StockAlertId = item.StockAlertId,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Sku = item.Product.Sku,
                UnitCode = item.Product.Unit.Code,
                WarehouseId = item.WarehouseId,
                WarehouseName = item.Warehouse.Name,
                AlertType = item.AlertType,
                CurrentStock = item.CurrentStock,
                Threshold = item.Threshold,
                SuggestedOrderQty = item.SuggestedOrderQty,
                Status = item.Status,
                CreatedAt = item.CreatedAt,
                ResolvedAt = item.ResolvedAt,
                ResolvedByName = item.ResolvedByNavigation != null ? item.ResolvedByNavigation.FullName : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetAlertRecipientEmailsAsync(string permissionCode, CancellationToken cancellationToken = default)
    {
        return await context.Users.AsNoTracking()
            .Where(user => user.IsActive
                && user.UserRoles.Any(userRole => userRole.Role.RolePermissions.Any(rolePermission => rolePermission.Permission.Code == permissionCode)))
            .Select(user => user.Email)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
