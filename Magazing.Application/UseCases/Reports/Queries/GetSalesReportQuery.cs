using Magazing.Application.DTOs.Reports;
using Magazing.Application.Services;
using MediatR;

namespace Magazing.Application.UseCases.Reports.Queries;

public record GetSalesReportQuery(DateOnly From, DateOnly To, string? GroupBy) : IRequest<SalesReportDto>;

internal sealed class GetSalesReportQueryHandler(IReportsReadService reportsReadService)
    : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        var groupBy = string.IsNullOrWhiteSpace(request.GroupBy) ? SalesAggregator.GroupByDay : request.GroupBy.Trim().ToLower();
        if (groupBy is not (SalesAggregator.GroupByDay or SalesAggregator.GroupByWeek or SalesAggregator.GroupByMonth))
        {
            throw new InvalidOperationException("groupBy debe ser day, week o month.");
        }

        var (fromUtc, toUtc) = SalesAggregator.Range(request.From, request.To);
        var lines = await reportsReadService.GetCompletedSaleLinesAsync(fromUtc, toUtc, cancellationToken);

        var byPeriod = lines
            .GroupBy(line => SalesAggregator.PeriodStart(SalesAggregator.LimaDate(line), groupBy))
            .ToDictionary(group => group.Key, group => group.ToList());

        var rows = new List<SalesReportRowDto>();
        for (var period = SalesAggregator.PeriodStart(request.From, groupBy);
             period <= request.To;
             period = SalesAggregator.NextPeriod(period, groupBy))
        {
            var summary = SalesAggregator.Summarize(byPeriod.GetValueOrDefault(period) ?? []);
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
            Totals = SalesAggregator.Summarize(lines)
        };
    }
}
