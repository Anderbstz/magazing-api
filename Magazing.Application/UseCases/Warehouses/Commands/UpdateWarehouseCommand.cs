using Magazing.Application.DTOs.Inventory;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Warehouses.Commands;

public record UpdateWarehouseCommand(Guid WarehouseId, string Name, string? Address, bool IsActive) : IRequest<WarehouseDto>;

internal sealed class UpdateWarehouseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
{
    public async Task<WarehouseDto> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre del almacen es obligatorio.");
        }

        var warehouse = await unitOfWork.Repository<Warehouse>().GetByIdAsync(request.WarehouseId, cancellationToken)
            ?? throw new KeyNotFoundException("El almacen no existe.");

        if (warehouse.IsActive && !request.IsActive
            && await unitOfWork.Repository<Stock>().AnyAsync(item => item.WarehouseId == request.WarehouseId && item.Quantity > 0, cancellationToken))
        {
            throw new InvalidOperationException("No se puede desactivar un almacen que todavia tiene stock.");
        }

        warehouse.Name = name;
        warehouse.Address = request.Address?.Trim();
        warehouse.IsActive = request.IsActive;
        warehouse.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return WarehouseReadModel.ToDto(warehouse);
    }
}
