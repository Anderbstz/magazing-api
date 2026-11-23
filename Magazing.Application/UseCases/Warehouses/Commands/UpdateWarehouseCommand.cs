using Magazing.Application.DTOs.Inventory;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Warehouses.Commands;

public record UpdateWarehouseCommand(
    Guid WarehouseId,
    string Name,
    string? Address,
    bool IsActive,
    Guid? BranchId = null,
    string? Code = null) : IRequest<WarehouseDto>;

internal sealed class UpdateWarehouseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
{
    private const int MaxCodeLength = 10;

    public async Task<WarehouseDto> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre del almacén es obligatorio.");
        }

        var warehouses = unitOfWork.Repository<Warehouse>();
        var warehouse = await warehouses.GetByIdAsync(request.WarehouseId, cancellationToken)
            ?? throw new KeyNotFoundException("El almacén no existe.");

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToUpper();
            if (code.Length > MaxCodeLength)
            {
                throw new InvalidOperationException($"El código debe tener como máximo {MaxCodeLength} caracteres.");
            }

            if (await warehouses.AnyAsync(item => item.Code == code && item.WarehouseId != request.WarehouseId, cancellationToken))
            {
                throw new InvalidOperationException("Ya existe un almacén con ese código.");
            }

            warehouse.Code = code;
        }

        if (request.BranchId is { } branchId && branchId != warehouse.BranchId)
        {
            if (!await unitOfWork.Repository<Branch>().AnyAsync(item => item.BranchId == branchId && item.IsActive, cancellationToken))
            {
                throw new InvalidOperationException("La sucursal no existe o está inactiva.");
            }

            warehouse.BranchId = branchId;
        }

        if (warehouse.IsActive && !request.IsActive
            && await unitOfWork.Repository<Stock>().AnyAsync(item => item.WarehouseId == request.WarehouseId && item.Quantity > 0, cancellationToken))
        {
            throw new InvalidOperationException("No se puede desactivar un almacén que todavía tiene stock.");
        }

        warehouse.Name = name;
        warehouse.Address = request.Address?.Trim();
        warehouse.IsActive = request.IsActive;
        warehouse.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return await WarehouseReadModel.ToDtoAsync(unitOfWork, warehouse, cancellationToken);
    }
}
