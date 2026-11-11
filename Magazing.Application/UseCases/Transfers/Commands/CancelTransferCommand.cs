using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Inventory;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Transfers.Commands;

public record CancelTransferCommand(Guid TransferId) : IRequest<TransferDetailDto>;

internal sealed class CancelTransferCommandHandler(IUnitOfWork unitOfWork, IInventoryReadService inventoryReadService)
    : IRequestHandler<CancelTransferCommand, TransferDetailDto>
{
    public async Task<TransferDetailDto> Handle(CancelTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await unitOfWork.Repository<Transfer>().GetByIdAsync(request.TransferId, cancellationToken)
            ?? throw new KeyNotFoundException("La transferencia no existe.");

        if (transfer.Status != TransferStatuses.Pending)
        {
            throw new InvalidOperationException($"Solo se puede cancelar una transferencia pendiente. Estado actual: {transfer.Status}.");
        }

        transfer.Status = TransferStatuses.Cancelled;
        transfer.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return await inventoryReadService.GetTransferAsync(transfer.TransferId, cancellationToken);
    }
}
