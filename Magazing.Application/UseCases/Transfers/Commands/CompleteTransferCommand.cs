using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Inventory;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Transfers.Commands;

public record CompleteTransferCommand(Guid TransferId, Guid UserId) : IRequest<TransferDetailDto>;

internal sealed class CompleteTransferCommandHandler(
    IUnitOfWork unitOfWork,
    InventoryLedger inventoryLedger,
    IInventoryReadService inventoryReadService)
    : IRequestHandler<CompleteTransferCommand, TransferDetailDto>
{
    public async Task<TransferDetailDto> Handle(CompleteTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await unitOfWork.Repository<Transfer>().GetByIdAsync(request.TransferId, cancellationToken)
            ?? throw new KeyNotFoundException("La transferencia no existe.");

        if (transfer.Status != TransferStatuses.Pending)
        {
            throw new InvalidOperationException($"Solo se puede completar una transferencia pendiente. Estado actual: {transfer.Status}.");
        }

        var details = await unitOfWork.Repository<TransferDetail>().FindAsync(item => item.TransferId == transfer.TransferId, cancellationToken);
        var notes = $"Transferencia {transfer.Code}";

        foreach (var detail in details)
        {
            await inventoryLedger.RegisterAsync(new InventoryMovementRequest(
                detail.ProductId,
                transfer.OriginWarehouseId,
                MovementTypeCodes.TransferOut,
                detail.Quantity,
                null,
                ReferenceTypes.Transfer,
                transfer.TransferId,
                notes,
                request.UserId), cancellationToken);

            await inventoryLedger.RegisterAsync(new InventoryMovementRequest(
                detail.ProductId,
                transfer.DestinationWarehouseId,
                MovementTypeCodes.TransferIn,
                detail.Quantity,
                null,
                ReferenceTypes.Transfer,
                transfer.TransferId,
                notes,
                request.UserId), cancellationToken);
        }

        var now = DateTime.UtcNow;
        transfer.Status = TransferStatuses.Completed;
        transfer.CompletedBy = request.UserId;
        transfer.CompletedAt = now;
        transfer.UpdatedAt = now;

        await unitOfWork.CompleteAsync(cancellationToken);

        return await inventoryReadService.GetTransferAsync(transfer.TransferId, cancellationToken);
    }
}
