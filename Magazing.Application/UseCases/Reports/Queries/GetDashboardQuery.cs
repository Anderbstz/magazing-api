using Magazing.Application.Services.Common;
using Magazing.Application.DTOs.Reports;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Reports.Queries;

public record GetDashboardQuery : IRequest<DashboardDto>;

internal sealed class GetDashboardQueryHandler(
    IUnitOfWork unitOfWork,
    IReportsReadService reportsReadService,
    IInventoryReadService inventoryReadService)
    : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private const int TrendDays = 30;
    private const int TopProductsLimit = 5;

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = BusinessTime.Today();
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var previousMonthStart = monthStart.AddMonths(-1);
        var previousMonthCut = previousMonthStart.AddDays(Math.Min(today.Day, DateTime.DaysInMonth(previousMonthStart.Year, previousMonthStart.Month)) - 1);
        var trendStart = today.AddDays(-(TrendDays - 1));
        var fetchStart = previousMonthStart < trendStart ? previousMonthStart : trendStart;

        var lines = await reportsReadService.GetCompletedSaleLinesAsync(
            BusinessTime.StartOfDayUtc(fetchStart),
            BusinessTime.StartOfNextDayUtc(today),
            cancellationToken);

        var dated = lines.Select(line => (Line: line, Date: SalesAggregator.LimaDate(line))).ToList();

        var todaySummary = SalesAggregator.Summarize(dated.Where(item => item.Date == today).Select(item => item.Line));
        var monthSummary = SalesAggregator.Summarize(dated.Where(item => item.Date >= monthStart).Select(item => item.Line));
        var previousMonthTotal = dated
            .Where(item => item.Date >= previousMonthStart && item.Date <= previousMonthCut)
            .Sum(item => item.Line.LineTotal);

        var byDay = dated
            .Where(item => item.Date >= trendStart)
            .GroupBy(item => item.Date)
            .ToDictionary(group => group.Key, group => SalesAggregator.Summarize(group.Select(item => item.Line)));

        var salesByDay = Enumerable.Range(0, TrendDays)
            .Select(offset => trendStart.AddDays(offset))
            .Select(date => new DailySalesDto
            {
                Date = date,
                Total = byDay.GetValueOrDefault(date)?.Total ?? 0,
                Count = byDay.GetValueOrDefault(date)?.Count ?? 0
            })
            .ToList();

        var dashboard = new DashboardDto
        {
            Today = new DashboardTodayDto
            {
                Total = todaySummary.Total,
                Count = todaySummary.Count,
                AverageTicket = todaySummary.Count == 0 ? 0 : Math.Round(todaySummary.Total / todaySummary.Count, 2)
            },
            Month = new DashboardMonthDto
            {
                Total = monthSummary.Total,
                Count = monthSummary.Count,
                GrossProfit = monthSummary.GrossProfit,
                MarginPct = monthSummary.Subtotal == 0 ? 0 : Math.Round(monthSummary.GrossProfit / monthSummary.Subtotal * 100, 2)
            },
            PreviousMonthTotal = previousMonthTotal,
            SalesByDay = salesByDay,
            TopProducts = SalesAggregator.TopProducts(dated.Where(item => item.Date >= trendStart).Select(item => item.Line), TopProductsLimit)
        };

        foreach (var warehouse in await unitOfWork.Repository<Warehouse>().FindAsync(item => item.IsActive, cancellationToken))
        {
            var summary = await inventoryReadService.GetStockSummaryAsync(warehouse.WarehouseId, cancellationToken);
            dashboard.InventoryValue += summary.TotalValue;
            dashboard.LowStockCount += summary.LowCount;
            dashboard.OutOfStockCount += summary.OutCount;
        }

        var counts = await reportsReadService.GetOperationalCountsAsync(cancellationToken);
        dashboard.OpenAlerts = counts.OpenAlerts;
        dashboard.PendingPurchaseOrders = counts.PendingPurchaseOrders;
        dashboard.PendingTransfers = counts.PendingTransfers;

        return dashboard;
    }
}
