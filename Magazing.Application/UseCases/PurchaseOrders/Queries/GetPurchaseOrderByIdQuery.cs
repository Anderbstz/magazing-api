using Magazing.Application.DTOs.Purchases;
using Magazing.Application.Services;
using MediatR;

namespace Magazing.Application.UseCases.PurchaseOrders.Queries;

public record GetPurchaseOrderByIdQuery(Guid PurchaseOrderId) : IRequest<PurchaseOrderDetailDto>;

internal sealed class GetPurchaseOrderByIdQueryHandler(IPurchaseReadService purchaseReadService)
    : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDetailDto>
{
    public Task<PurchaseOrderDetailDto> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return purchaseReadService.GetOrderAsync(request.PurchaseOrderId, cancellationToken);
    }
}
