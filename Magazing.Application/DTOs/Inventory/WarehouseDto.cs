namespace Magazing.Application.DTOs.Inventory;

public class WarehouseDto
{
    public Guid WarehouseId { get; set; }

    public Guid BranchId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public bool IsActive { get; set; }
}
