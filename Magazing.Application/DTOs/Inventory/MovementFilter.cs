namespace Magazing.Application.DTOs.Inventory;

public record MovementFilter(
    Guid? ProductId,
    Guid? WarehouseId,
    string? MovementTypeCode,
    DateTime? FromUtc,
    DateTime? ToUtcExclusive,
    int Page,
    int PageSize);
