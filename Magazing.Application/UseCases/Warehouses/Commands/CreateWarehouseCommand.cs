using Magazing.Application.DTOs.Inventory;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Warehouses.Commands;

public record CreateWarehouseCommand(Guid BranchId, string Code, string Name, string? Address) : IRequest<WarehouseDto>;

internal sealed class CreateWarehouseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    private const int MaxCodeLength = 10;

    public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code?.Trim().ToUpper() ?? string.Empty;
        var name = request.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(code) || code.Length > MaxCodeLength)
        {
            throw new InvalidOperationException($"El codigo es obligatorio y debe tener como maximo {MaxCodeLength} caracteres.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre del almacen es obligatorio.");
        }

        if (!await unitOfWork.Repository<Branch>().AnyAsync(item => item.BranchId == request.BranchId && item.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("La sucursal no existe o esta inactiva.");
        }

        var warehouses = unitOfWork.Repository<Warehouse>();
        if (await warehouses.AnyAsync(item => item.Code == code, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un almacen con ese codigo.");
        }

        var warehouse = new Warehouse
        {
            WarehouseId = Guid.NewGuid(),
            BranchId = request.BranchId,
            Code = code,
            Name = name,
            Address = request.Address?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await warehouses.AddAsync(warehouse, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return WarehouseReadModel.ToDto(warehouse);
    }
}
