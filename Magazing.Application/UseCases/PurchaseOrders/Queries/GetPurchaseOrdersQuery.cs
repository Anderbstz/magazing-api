using Magazing.Application.DTOs.Purchases;
using Magazing.Application.Services;
using Magazing.Domain.Common;
using Magazing.Domain.Purchases;
using MediatR;

namespace Magazing.Application.UseCases.PurchaseOrders.Queries;

public record GetPurchaseOrdersQuery(
    string? Status = null,
    Guid? SupplierId = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<PurchaseOrderSummaryDto>>;

internal sealed class GetPurchaseOrdersQueryHandler(IPurchaseReadService purchaseReadService)
    : IRequestHandler<GetPurchaseOrdersQuery, PagedResult<PurchaseOrderSummaryDto>>
{
    public Task<PagedResult<PurchaseOrderSummaryDto>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim().ToUpper();
        if (status is not null && !PurchaseOrderStatuses.IsValid(status))
        {
            throw new InvalidOperationException("El estado debe ser DRAFT, APPROVED, RECEIVED o CANCELLED.");
        }

        return purchaseReadService.GetOrdersAsync(status, request.SupplierId, request.Search, request.Page, request.PageSize, cancellationToken);
    }
}
