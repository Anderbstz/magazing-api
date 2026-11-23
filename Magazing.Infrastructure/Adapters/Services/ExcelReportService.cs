using ClosedXML.Excel;
using Magazing.Application.Common;
using Magazing.Application.DTOs.Inventory;
using Magazing.Application.DTOs.Reports;
using Magazing.Application.Inventory;
using Magazing.Application.Services;

namespace Magazing.Infrastructure.Adapters.Services;

public class ExcelReportService : IExcelReportService
{
    private const string MoneyFormat = "\"S/\" #,##0.00";
    private const string CostFormat = "\"S/\" #,##0.0000";
    private const string QuantityFormat = "#,##0.###";
    private const string PercentFormat = "0.00\"%\"";
    private const string DateFormat = "dd/mm/yyyy";
    private const string DateTimeFormat = "dd/mm/yyyy hh:mm";

    public byte[] BuildSalesReport(SalesReportDto report, string subtitle, string groupBy)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Ventas");

        var headers = new[] { "Periodo", "Comprobantes", "Op. gravada", "IGV", "Total", "Costo", "Utilidad", "Margen" };
        WriteTitle(worksheet, "Reporte de ventas", subtitle, headers.Length);
        WriteHeaders(worksheet, headers);

        var row = 4;
        foreach (var item in report.Rows)
        {
            worksheet.Cell(row, 1).Value = item.Period.ToDateTime(TimeOnly.MinValue);
            worksheet.Cell(row, 2).Value = item.Count;
            worksheet.Cell(row, 3).Value = item.Subtotal;
            worksheet.Cell(row, 4).Value = item.TaxAmount;
            worksheet.Cell(row, 5).Value = item.Total;
            worksheet.Cell(row, 6).Value = item.Cost;
            worksheet.Cell(row, 7).Value = item.GrossProfit;
            worksheet.Cell(row, 8).Value = Margin(item.GrossProfit, item.Subtotal);
            row++;
        }

        CreateTable(worksheet, row - 1, headers.Length, "TablaVentas");

        var totals = report.Totals;
        worksheet.Cell(row, 1).Value = "Total";
        worksheet.Cell(row, 2).Value = totals.Count;
        worksheet.Cell(row, 3).Value = totals.Subtotal;
        worksheet.Cell(row, 4).Value = totals.TaxAmount;
        worksheet.Cell(row, 5).Value = totals.Total;
        worksheet.Cell(row, 6).Value = totals.Cost;
        worksheet.Cell(row, 7).Value = totals.GrossProfit;
        worksheet.Cell(row, 8).Value = Margin(totals.GrossProfit, totals.Subtotal);
        StyleTotalRow(worksheet, row, 1, headers.Length);

        worksheet.Range(4, 1, row - 1, 1).Style.NumberFormat.Format = groupBy == "month" ? "mmm yyyy" : DateFormat;
        worksheet.Range(4, 3, row, 7).Style.NumberFormat.Format = MoneyFormat;
        worksheet.Range(4, 8, row, 8).Style.NumberFormat.Format = PercentFormat;
        worksheet.Columns().AdjustToContents();

        return ToBytes(workbook);
    }

    public byte[] BuildStockReport(string subtitle, StockSummaryDto summary, IReadOnlyList<StockItemDto> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Stock");

        var headers = new[] { "SKU", "Producto", "Categoría", "Unidad", "Stock", "Mínimo", "Máximo", "Costo prom.", "Valor", "Estado" };
        WriteTitle(worksheet, "Stock valorizado", subtitle, headers.Length);
        WriteHeaders(worksheet, headers);

        var row = 4;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.Sku;
            worksheet.Cell(row, 2).Value = item.ProductName;
            worksheet.Cell(row, 3).Value = item.CategoryName;
            worksheet.Cell(row, 4).Value = item.UnitCode;
            worksheet.Cell(row, 5).Value = item.Quantity;
            worksheet.Cell(row, 6).Value = item.MinStock;
            worksheet.Cell(row, 7).Value = item.MaxStock is { } max ? max : Blank.Value;
            worksheet.Cell(row, 8).Value = item.AverageCost;
            worksheet.Cell(row, 9).Value = item.StockValue;
            worksheet.Cell(row, 10).Value = StatusLabel(item.Status);
            row++;
        }

        CreateTable(worksheet, row - 1, headers.Length, "TablaStock");

        worksheet.Cell(row, 8).Value = "Total";
        worksheet.Cell(row, 9).Value = items.Sum(item => item.StockValue);
        StyleTotalRow(worksheet, row, 8, 9);

        worksheet.Range(4, 5, row, 7).Style.NumberFormat.Format = QuantityFormat;
        worksheet.Range(4, 8, row - 1, 8).Style.NumberFormat.Format = CostFormat;
        worksheet.Range(4, 9, row, 9).Style.NumberFormat.Format = MoneyFormat;
        worksheet.Columns().AdjustToContents();

        var summarySheet = workbook.Worksheets.Add("Resumen");
        WriteTitle(summarySheet, "Resumen del almacén", subtitle, 2);
        var summaryRows = new (string Label, decimal Value)[]
        {
            ("Productos", summary.ProductCount),
            ("Unidades", summary.TotalUnits),
            ("Valor del inventario", summary.TotalValue),
            ("Bajo mínimo", summary.LowCount),
            ("Sin stock", summary.OutCount)
        };

        for (var index = 0; index < summaryRows.Length; index++)
        {
            summarySheet.Cell(index + 3, 1).Value = summaryRows[index].Label;
            summarySheet.Cell(index + 3, 2).Value = summaryRows[index].Value;
            summarySheet.Cell(index + 3, 1).Style.Font.Bold = true;
        }

        summarySheet.Cell(5, 2).Style.NumberFormat.Format = MoneyFormat;
        summarySheet.Columns().AdjustToContents();

        return ToBytes(workbook);
    }

    public byte[] BuildKardexReport(string subtitle, IReadOnlyList<MovementDto> movements)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Kardex");

        var headers = new[]
        {
            "Fecha", "SKU", "Producto", "Almacén", "Movimiento", "Documento",
            "Entrada", "Salida", "Saldo", "Costo unit.", "Costo prom.", "Usuario", "Notas"
        };
        WriteTitle(worksheet, "Kardex de inventario", subtitle, headers.Length);
        WriteHeaders(worksheet, headers);

        var row = 4;
        foreach (var movement in movements.OrderBy(item => item.CreatedAt))
        {
            worksheet.Cell(row, 1).Value = BusinessTime.ToLima(movement.CreatedAt);
            worksheet.Cell(row, 2).Value = movement.Sku;
            worksheet.Cell(row, 3).Value = movement.ProductName;
            worksheet.Cell(row, 4).Value = movement.WarehouseCode;
            worksheet.Cell(row, 5).Value = movement.MovementTypeName;
            worksheet.Cell(row, 6).Value = movement.ReferenceCode ?? string.Empty;
            worksheet.Cell(row, 7).Value = movement.Direction > 0 ? movement.Quantity : Blank.Value;
            worksheet.Cell(row, 8).Value = movement.Direction < 0 ? movement.Quantity : Blank.Value;
            worksheet.Cell(row, 9).Value = movement.BalanceAfter;
            worksheet.Cell(row, 10).Value = movement.UnitCost;
            worksheet.Cell(row, 11).Value = movement.AverageCostAfter;
            worksheet.Cell(row, 12).Value = movement.CreatedByName;
            worksheet.Cell(row, 13).Value = movement.Notes ?? string.Empty;
            row++;
        }

        CreateTable(worksheet, row - 1, headers.Length, "TablaKardex");

        var lastDataRow = Math.Max(row - 1, 4);
        worksheet.Range(4, 1, lastDataRow, 1).Style.NumberFormat.Format = DateTimeFormat;
        worksheet.Range(4, 7, lastDataRow, 9).Style.NumberFormat.Format = QuantityFormat;
        worksheet.Range(4, 10, lastDataRow, 11).Style.NumberFormat.Format = CostFormat;
        worksheet.Columns().AdjustToContents();

        return ToBytes(workbook);
    }

    private static decimal Margin(decimal grossProfit, decimal subtotal)
    {
        return subtotal == 0 ? 0 : Math.Round(grossProfit / subtotal * 100, 2);
    }

    private static string StatusLabel(string status)
    {
        return status switch
        {
            StockStatus.Out => "Sin stock",
            StockStatus.Low => "Bajo mínimo",
            _ => "Normal"
        };
    }

    private static void WriteTitle(IXLWorksheet worksheet, string title, string subtitle, int columns)
    {
        var titleRange = worksheet.Range(1, 1, 1, columns);
        titleRange.Merge();
        titleRange.Value = $"Magazing · {title}";
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 14;
        titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var subtitleRange = worksheet.Range(2, 1, 2, columns);
        subtitleRange.Merge();
        subtitleRange.Value = $"{subtitle} · generado el {BusinessTime.ToLima(DateTime.UtcNow):dd/MM/yyyy HH:mm}";
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontColor = XLColor.Gray;
        subtitleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private static void WriteHeaders(IXLWorksheet worksheet, string[] headers)
    {
        for (var column = 0; column < headers.Length; column++)
        {
            worksheet.Cell(3, column + 1).Value = headers[column];
        }
    }

    private static void CreateTable(IXLWorksheet worksheet, int lastRow, int columns, string tableName)
    {
        var range = worksheet.Range(3, 1, Math.Max(lastRow, 4), columns);
        var table = range.CreateTable(tableName);
        table.Theme = XLTableTheme.TableStyleMedium2;
    }

    private static void StyleTotalRow(IXLWorksheet worksheet, int row, int firstColumn, int lastColumn)
    {
        var totalRange = worksheet.Range(row, firstColumn, row, lastColumn);
        totalRange.Style.Font.Bold = true;
        totalRange.Style.Fill.BackgroundColor = XLColor.LightGray;
    }

    private static byte[] ToBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
