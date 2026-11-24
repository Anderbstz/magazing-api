using Magazing.Application.DTOs.Catalog;
using Magazing.Application.DTOs.Inventory;
using Magazing.Application.DTOs.Reports;

namespace Magazing.Application.Services;

public interface IExcelReportService
{
    byte[] BuildSalesReport(SalesReportDto report, string subtitle, string groupBy);

    byte[] BuildStockReport(string subtitle, StockSummaryDto summary, IReadOnlyList<StockItemDto> items);

    byte[] BuildKardexReport(string subtitle, IReadOnlyList<MovementDto> movements);

    byte[] BuildProductImportTemplate(ProductImportCatalogs catalogs);
}
