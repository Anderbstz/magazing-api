using Magazing.Application.Common;
using Magazing.Application.DTOs.Reports;

namespace Magazing.Application.UseCases.Reports;

internal static class SalesAggregator
{
    public const string GroupByDay = "day";
    public const string GroupByWeek = "week";
    public const string GroupByMonth = "month";

    public static DateOnly LimaDate(SaleLine line)
    {
        return DateOnly.FromDateTime(BusinessTime.ToLima(line.SoldAt));
    }

    public static DateOnly PeriodStart(DateOnly date, string groupBy)
    {
        return groupBy switch
        {
            GroupByWeek => BusinessTime.WeekStart(date),
            GroupByMonth => new DateOnly(date.Year, date.Month, 1),
            _ => date
        };
    }

    public static DateOnly NextPeriod(DateOnly period, string groupBy)
    {
        return groupBy switch
        {
            GroupByWeek => period.AddDays(7),
            GroupByMonth => period.AddMonths(1),
            _ => period.AddDays(1)
        };
    }

    public static SalesReportTotalsDto Summarize(IEnumerable<SaleLine> lines)
    {
        var list = lines.ToList();
        var subtotal = list.Sum(line => line.LineTotal - line.TaxAmount);
        var cost = Math.Round(list.Sum(line => line.Quantity * line.UnitCost), 2);

        return new SalesReportTotalsDto
        {
            Count = list.Select(line => line.SaleId).Distinct().Count(),
            Subtotal = subtotal,
            TaxAmount = list.Sum(line => line.TaxAmount),
            Total = list.Sum(line => line.LineTotal),
            Cost = cost,
            GrossProfit = subtotal - cost
        };
    }

    public static List<TopProductDto> TopProducts(IEnumerable<SaleLine> lines, int limit)
    {
        return lines
            .GroupBy(line => new { line.ProductId, line.Sku, line.ProductName })
            .Select(group => new TopProductDto
            {
                ProductId = group.Key.ProductId,
                Sku = group.Key.Sku,
                ProductName = group.Key.ProductName,
                Quantity = group.Sum(line => line.Quantity),
                Total = group.Sum(line => line.LineTotal),
                GrossProfit = Math.Round(group.Sum(line => line.LineTotal - line.TaxAmount - line.Quantity * line.UnitCost), 2)
            })
            .OrderByDescending(item => item.Total)
            .ThenBy(item => item.ProductName)
            .Take(limit)
            .ToList();
    }

    public static string NormalizeGroupBy(string? groupBy)
    {
        var value = string.IsNullOrWhiteSpace(groupBy) ? GroupByDay : groupBy.Trim().ToLower();
        if (value is not (GroupByDay or GroupByWeek or GroupByMonth))
        {
            throw new InvalidOperationException("groupBy debe ser day, week o month.");
        }

        return value;
    }

    public static SalesReportDto BuildReport(IReadOnlyList<SaleLine> lines, DateOnly from, DateOnly to, string groupBy)
    {
        var byPeriod = lines
            .GroupBy(line => PeriodStart(LimaDate(line), groupBy))
            .ToDictionary(group => group.Key, group => group.ToList());

        var rows = new List<SalesReportRowDto>();
        for (var period = PeriodStart(from, groupBy); period <= to; period = NextPeriod(period, groupBy))
        {
            var summary = Summarize(byPeriod.GetValueOrDefault(period) ?? []);
            rows.Add(new SalesReportRowDto
            {
                Period = period,
                Count = summary.Count,
                Subtotal = summary.Subtotal,
                TaxAmount = summary.TaxAmount,
                Total = summary.Total,
                Cost = summary.Cost,
                GrossProfit = summary.GrossProfit
            });
        }

        return new SalesReportDto
        {
            Rows = rows,
            Totals = Summarize(lines)
        };
    }

    public static (DateTime FromUtc, DateTime ToUtcExclusive) Range(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new InvalidOperationException("La fecha inicial no puede ser mayor que la fecha final.");
        }

        if (to.DayNumber - from.DayNumber > 731)
        {
            throw new InvalidOperationException("El rango maximo es de 2 anios.");
        }

        return (BusinessTime.StartOfDayUtc(from), BusinessTime.StartOfNextDayUtc(to));
    }
}
