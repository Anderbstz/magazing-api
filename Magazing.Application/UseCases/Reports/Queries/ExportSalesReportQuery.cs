using Magazing.Application.DTOs.Reports;
using Magazing.Application.Services;
using MediatR;

namespace Magazing.Application.UseCases.Reports.Queries;

public record ExportSalesReportQuery(DateOnly From, DateOnly To, string? GroupBy) : IRequest<ExportFile>;

internal sealed class ExportSalesReportQueryHandler(IReportsReadService reportsReadService, IExcelReportService excelReportService)
    : IRequestHandler<ExportSalesReportQuery, ExportFile>
{
    public async Task<ExportFile> Handle(ExportSalesReportQuery request, CancellationToken cancellationToken)
    {
        var groupBy = SalesAggregator.NormalizeGroupBy(request.GroupBy);
        var (fromUtc, toUtc) = SalesAggregator.Range(request.From, request.To);
        var lines = await reportsReadService.GetCompletedSaleLinesAsync(fromUtc, toUtc, cancellationToken);
        var report = SalesAggregator.BuildReport(lines, request.From, request.To, groupBy);

        var subtitle = $"Del {request.From:dd/MM/yyyy} al {request.To:dd/MM/yyyy} · {ExportNames.GroupByLabel(groupBy)}";
        var content = excelReportService.BuildSalesReport(report, subtitle, groupBy);

        return new ExportFile(content, $"ventas_{request.From:yyyy-MM-dd}_{request.To:yyyy-MM-dd}.xlsx");
    }
}
