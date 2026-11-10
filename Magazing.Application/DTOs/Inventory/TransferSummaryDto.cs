namespace Magazing.Application.DTOs.Inventory;

public class TransferSummaryDto
{
    public Guid TransferId { get; set; }

    public string Code { get; set; } = null!;

    public Guid OriginWarehouseId { get; set; }

    public string OriginWarehouseName { get; set; } = null!;

    public Guid DestinationWarehouseId { get; set; }

    public string DestinationWarehouseName { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int ItemCount { get; set; }

    public decimal TotalQuantity { get; set; }

    public string? Notes { get; set; }

    public string RequestedByName { get; set; } = null!;

    public string? CompletedByName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
