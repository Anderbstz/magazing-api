namespace Magazing.Domain.Services.Inventory;

public record InventoryMovementRequest(
    Guid ProductId,
    Guid WarehouseId,
    string MovementTypeCode,
    decimal Quantity,
    decimal? UnitCost,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Notes,
    Guid? UserId);
