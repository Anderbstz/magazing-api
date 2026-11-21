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

    public static async Task<WarehouseDto> ToDtoAsync(IUnitOfWork unitOfWork, Warehouse warehouse, CancellationToken cancellationToken)
    {
        return (await ToDtosAsync(unitOfWork, [warehouse], cancellationToken))[0];
    }

    public static async Task<List<WarehouseDto>> ToDtosAsync(IUnitOfWork unitOfWork, IReadOnlyList<Warehouse> warehouses, CancellationToken cancellationToken)
    {
        var warehouseIds = warehouses.Select(item => item.WarehouseId).ToList();
        var branchIds = warehouses.Select(item => item.BranchId).Distinct().ToList();

        var branchNames = (await unitOfWork.Repository<Branch>().FindAsync(item => branchIds.Contains(item.BranchId), cancellationToken))
            .ToDictionary(item => item.BranchId, item => item.Name);

        var productsWithStock = (await unitOfWork.Repository<Stock>().FindAsync(
                item => warehouseIds.Contains(item.WarehouseId) && item.Quantity > 0,
                cancellationToken))
            .GroupBy(item => item.WarehouseId)
            .ToDictionary(group => group.Key, group => group.Count());

        return warehouses.Select(warehouse => new WarehouseDto
        {
            WarehouseId = warehouse.WarehouseId,
            BranchId = warehouse.BranchId,
            BranchName = branchNames.GetValueOrDefault(warehouse.BranchId, string.Empty),
            Code = warehouse.Code,
            Name = warehouse.Name,
            Address = warehouse.Address,
            IsActive = warehouse.IsActive,
            ProductsWithStock = productsWithStock.GetValueOrDefault(warehouse.WarehouseId)
        }).ToList();
    }
}
