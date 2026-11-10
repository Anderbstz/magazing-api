namespace Magazing.Application.DTOs.Inventory;

public record AdjustmentRequest(
    Guid ProductId,
    Guid WarehouseId,
    string Type,
    decimal Quantity,
    decimal? UnitCost,
    string Reason);
