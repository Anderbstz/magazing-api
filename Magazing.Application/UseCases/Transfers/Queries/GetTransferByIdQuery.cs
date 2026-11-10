using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Services;
using MediatR;

namespace Magazing.Application.UseCases.Transfers.Queries;

public record GetTransferByIdQuery(Guid TransferId) : IRequest<TransferDetailDto>;

internal sealed class GetTransferByIdQueryHandler(IInventoryReadService inventoryReadService)
    : IRequestHandler<GetTransferByIdQuery, TransferDetailDto>
{
    public Task<TransferDetailDto> Handle(GetTransferByIdQuery request, CancellationToken cancellationToken)
    {
        return inventoryReadService.GetTransferAsync(request.TransferId, cancellationToken);
    }
}
