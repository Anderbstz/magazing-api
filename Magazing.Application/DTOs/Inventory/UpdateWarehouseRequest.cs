namespace Magazing.Application.DTOs.Inventory;

public record UpdateWarehouseRequest(string Name, string? Address, bool IsActive);
