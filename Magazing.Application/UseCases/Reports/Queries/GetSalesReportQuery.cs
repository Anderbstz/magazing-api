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
        var groupBy = SalesAggregator.NormalizeGroupBy(request.GroupBy);
        var (fromUtc, toUtc) = SalesAggregator.Range(request.From, request.To);
        var lines = await reportsReadService.GetCompletedSaleLinesAsync(fromUtc, toUtc, cancellationToken);

        return SalesAggregator.BuildReport(lines, request.From, request.To, groupBy);
    }
}
