namespace Magazing.Application.DTOs.Inventory;

public class MovementDto
{
    public Guid InventoryMovementId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public string WarehouseCode { get; set; } = null!;

    public string MovementTypeCode { get; set; } = null!;

    public string MovementTypeName { get; set; } = null!;

    public short Direction { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal BalanceAfter { get; set; }

    public decimal AverageCostAfter { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public string? ReferenceCode { get; set; }

    public string? Notes { get; set; }

    public string CreatedByName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
