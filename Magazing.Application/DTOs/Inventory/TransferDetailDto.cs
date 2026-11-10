namespace Magazing.Application.DTOs.Inventory;

public class TransferDetailDto : TransferSummaryDto
{
    public List<TransferItemDto> Items { get; set; } = [];
}

public class TransferItemDto
{
    public Guid ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string UnitCode { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal AvailableAtOrigin { get; set; }
}
