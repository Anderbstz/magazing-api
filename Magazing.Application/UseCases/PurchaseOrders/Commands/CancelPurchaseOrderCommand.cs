using Magazing.Application.DTOs.Purchases;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Purchases;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.PurchaseOrders.Commands;

public record CancelPurchaseOrderCommand(Guid PurchaseOrderId) : IRequest<PurchaseOrderDetailDto>;

internal sealed class CancelPurchaseOrderCommandHandler(IUnitOfWork unitOfWork, IPurchaseReadService purchaseReadService)
    : IRequestHandler<CancelPurchaseOrderCommand, PurchaseOrderDetailDto>
{
    public async Task<PurchaseOrderDetailDto> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException("La orden de compra no existe.");

        if (order.Status is not (PurchaseOrderStatuses.Draft or PurchaseOrderStatuses.Approved))
        {
            throw new InvalidOperationException($"Solo se puede cancelar una orden en borrador o aprobada. Estado actual: {order.Status}.");
        }

        order.Status = PurchaseOrderStatuses.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return await purchaseReadService.GetOrderAsync(order.PurchaseOrderId, cancellationToken);
    }
}
