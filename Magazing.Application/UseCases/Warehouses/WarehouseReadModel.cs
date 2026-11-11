using Magazing.Application.DTOs.Inventory;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;

namespace Magazing.Application.UseCases.Warehouses;

internal static class WarehouseReadModel
{
    public static async Task<Warehouse> GetEntityAsync(IUnitOfWork unitOfWork, Guid warehouseId, CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<Warehouse>().FirstOrDefaultAsync(item => item.WarehouseId == warehouseId, cancellationToken)
            ?? throw new KeyNotFoundException("El almacen no existe.");
    }

    public static WarehouseDto ToDto(Warehouse warehouse)
    {
        return new WarehouseDto
        {
            WarehouseId = warehouse.WarehouseId,
            BranchId = warehouse.BranchId,
            Code = warehouse.Code,
            Name = warehouse.Name,
            Address = warehouse.Address,
            IsActive = warehouse.IsActive
        };
    }
}
