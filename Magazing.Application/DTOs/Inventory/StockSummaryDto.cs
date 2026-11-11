namespace Magazing.Application.DTOs.Inventory;

public class StockSummaryDto
{
    public int ProductCount { get; set; }

    public decimal TotalUnits { get; set; }

    public decimal TotalValue { get; set; }

    public int LowCount { get; set; }

    public int OutCount { get; set; }
}
