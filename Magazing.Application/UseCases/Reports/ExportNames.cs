using Magazing.Application.Common;

namespace Magazing.Application.UseCases.Reports;

internal static class ExportNames
{
    public const int PageSize = 100;
    public const int MaxRows = 20000;

    public static string GroupByLabel(string groupBy)
    {
        return groupBy switch
        {
            SalesAggregator.GroupByWeek => "por semana",
            SalesAggregator.GroupByMonth => "por mes",
            _ => "por dia"
        };
    }

    public static string Stamp()
    {
        return $"{BusinessTime.Today():yyyy-MM-dd}";
    }
}
