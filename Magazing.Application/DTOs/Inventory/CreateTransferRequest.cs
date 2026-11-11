namespace Magazing.Application.DTOs.Inventory;

public record CreateTransferRequest(
    Guid OriginWarehouseId,
    Guid DestinationWarehouseId,
    string? Notes,
    List<TransferItemRequest> Items);

public record TransferItemRequest(Guid ProductId, decimal Quantity);
