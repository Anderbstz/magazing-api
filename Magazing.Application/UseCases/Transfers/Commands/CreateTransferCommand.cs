using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Inventory;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Transfers.Commands;

public record CreateTransferCommand(
    Guid OriginWarehouseId,
    Guid DestinationWarehouseId,
    string? Notes,
    List<TransferItemRequest> Items,
    Guid UserId) : IRequest<TransferDetailDto>;

internal sealed class CreateTransferCommandHandler(IUnitOfWork unitOfWork, IInventoryReadService inventoryReadService)
    : IRequestHandler<CreateTransferCommand, TransferDetailDto>
{
    public async Task<TransferDetailDto> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        var items = request.Items ?? [];

        if (request.OriginWarehouseId == request.DestinationWarehouseId)
        {
            throw new InvalidOperationException("El almacen de origen y el de destino deben ser distintos.");
        }

        if (items.Count == 0)
        {
            throw new InvalidOperationException("La transferencia debe tener al menos un producto.");
        }

        if (items.Any(item => item.Quantity <= 0))
        {
            throw new InvalidOperationException("Todas las cantidades deben ser mayores a cero.");
        }

        var productIds = items.Select(item => item.ProductId).ToList();
        if (productIds.Distinct().Count() != productIds.Count)
        {
            throw new InvalidOperationException("Un producto no puede repetirse en la misma transferencia.");
        }

        var warehouseIds = new List<Guid> { request.OriginWarehouseId, request.DestinationWarehouseId };
        var activeWarehouses = (await unitOfWork.Repository<Warehouse>().FindAsync(
            item => warehouseIds.Contains(item.WarehouseId) && item.IsActive,
            cancellationToken)).Count();

        if (activeWarehouses != 2)
        {
            throw new InvalidOperationException("El almacen de origen o de destino no existe o esta inactivo.");
        }

        var activeProducts = (await unitOfWork.Repository<Product>().FindAsync(
            item => productIds.Contains(item.ProductId) && item.IsActive,
            cancellationToken)).Count();

        if (activeProducts != productIds.Count)
        {
            throw new InvalidOperationException("Uno o mas productos no existen o estan inactivos.");
        }

        var transfers = unitOfWork.Repository<Transfer>();
        var sequence = (await transfers.GetAllAsync(cancellationToken)).Count() + 1;
        var now = DateTime.UtcNow;

        var transfer = new Transfer
        {
            TransferId = Guid.NewGuid(),
            Code = $"TR-{sequence:D6}",
            OriginWarehouseId = request.OriginWarehouseId,
            DestinationWarehouseId = request.DestinationWarehouseId,
            Status = TransferStatuses.Pending,
            Notes = request.Notes?.Trim(),
            RequestedBy = request.UserId,
            CreatedAt = now
        };

        await transfers.AddAsync(transfer, cancellationToken);

        foreach (var item in items)
        {
            await unitOfWork.Repository<TransferDetail>().AddAsync(new TransferDetail
            {
                TransferDetailId = Guid.NewGuid(),
                TransferId = transfer.TransferId,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        return await inventoryReadService.GetTransferAsync(transfer.TransferId, cancellationToken);
    }
}
