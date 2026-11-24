using Magazing.Application.DTOs.Reports;
using Magazing.Application.Services;
using Magazing.Domain.Services.Forecasting;
using Magazing.Domain.Services.Inventory;
using Magazing.Domain.Services.Purchases;
using Magazing.Domain.Services.Sales;
using Magazing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Magazing.Infrastructure.Adapters.Services;

public class ReportsReadService(MagazingDbContext context) : IReportsReadService
{
    public async Task<List<SaleLine>> GetCompletedSaleLinesAsync(DateTime fromUtc, DateTime toUtcExclusive, CancellationToken cancellationToken = default)
    {
        return await context.SaleDetails.AsNoTracking()
            .Where(item => item.Sale.Status == SaleStatuses.Completed
                && item.Sale.SoldAt >= fromUtc
                && item.Sale.SoldAt < toUtcExclusive)
            .Select(item => new SaleLine(
                item.SaleId,
                item.Sale.SoldAt,
                item.ProductId,
                item.Product.Sku,
                item.Product.Name,
                item.Quantity,
                item.LineTotal,
                item.TaxAmount,
                item.UnitCost))
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationalCounts> GetOperationalCountsAsync(CancellationToken cancellationToken = default)
    {
        var openAlerts = await context.StockAlerts.CountAsync(item => item.Status == AlertStatuses.Open, cancellationToken);

        var pendingOrders = await context.PurchaseOrders.CountAsync(
            item => item.Status == PurchaseOrderStatuses.Draft || item.Status == PurchaseOrderStatuses.Approved,
            cancellationToken);

        var pendingTransfers = await context.Transfers.CountAsync(item => item.Status == TransferStatuses.Pending, cancellationToken);

        return new OperationalCounts(openAlerts, pendingOrders, pendingTransfers);
    }
}
