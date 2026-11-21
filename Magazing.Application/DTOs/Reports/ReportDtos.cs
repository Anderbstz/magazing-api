namespace Magazing.Application.DTOs.Reports;

public record SaleLine(
    Guid SaleId,
    DateTime SoldAt,
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal Quantity,
    decimal LineTotal,
    decimal TaxAmount,
    decimal UnitCost);

public class SalesReportRowDto
{
    public DateOnly Period { get; set; }

    public int Count { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }

    public decimal Cost { get; set; }

    public decimal GrossProfit { get; set; }
}

public class SalesReportTotalsDto
{
    public int Count { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }

    public decimal Cost { get; set; }

    public decimal GrossProfit { get; set; }
}

public class SalesReportDto
{
    public List<SalesReportRowDto> Rows { get; set; } = [];

    public SalesReportTotalsDto Totals { get; set; } = new();
}

public class TopProductDto
{
    public Guid ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal Total { get; set; }

    public decimal GrossProfit { get; set; }
}

public class DashboardDto
{
    public DashboardTodayDto Today { get; set; } = new();

    public DashboardMonthDto Month { get; set; } = new();

    public decimal PreviousMonthTotal { get; set; }

    public decimal InventoryValue { get; set; }

    public int LowStockCount { get; set; }

    public int OutOfStockCount { get; set; }

    public int OpenAlerts { get; set; }

    public int PendingPurchaseOrders { get; set; }

    public int PendingTransfers { get; set; }

    public List<DailySalesDto> SalesByDay { get; set; } = [];

    public List<TopProductDto> TopProducts { get; set; } = [];
}

public class DashboardTodayDto
{
    public decimal Total { get; set; }

    public int Count { get; set; }

    public decimal AverageTicket { get; set; }
}

public class DashboardMonthDto
{
    public decimal Total { get; set; }

    public int Count { get; set; }

    public decimal GrossProfit { get; set; }

    public decimal MarginPct { get; set; }
}

public class DailySalesDto
{
    public DateOnly Date { get; set; }

    public decimal Total { get; set; }

    public int Count { get; set; }
}

public record OperationalCounts(int OpenAlerts, int PendingPurchaseOrders, int PendingTransfers);
