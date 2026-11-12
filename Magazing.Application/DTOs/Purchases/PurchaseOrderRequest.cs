namespace Magazing.Application.DTOs.Purchases;

public record PurchaseOrderRequest(
    Guid SupplierId,
    Guid WarehouseId,
    DateOnly? ExpectedDate,
    string? Notes,
    List<PurchaseOrderItemRequest> Items);

public record PurchaseOrderItemRequest(Guid ProductId, decimal Quantity, decimal UnitCost);
