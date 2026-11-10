using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Common;
using Magazing.Domain.Inventory;
using MediatR;

namespace Magazing.Application.UseCases.Transfers.Queries;

public record GetTransfersQuery(string? Status = null, int Page = 1, int PageSize = 20) : IRequest<PagedResult<TransferSummaryDto>>;

internal sealed class GetTransfersQueryHandler(IInventoryReadService inventoryReadService)
    : IRequestHandler<GetTransfersQuery, PagedResult<TransferSummaryDto>>
{
    public Task<PagedResult<TransferSummaryDto>> Handle(GetTransfersQuery request, CancellationToken cancellationToken)
    {
        var status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim().ToUpper();
        if (status is not null && !TransferStatuses.IsValid(status))
        {
            throw new InvalidOperationException("El estado debe ser PENDING, COMPLETED o CANCELLED.");
        }

        return inventoryReadService.GetTransfersAsync(status, request.Page, request.PageSize, cancellationToken);
    }
}
