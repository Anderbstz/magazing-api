using Magazing.Application.DTOs.Purchases;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Purchases;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.PurchaseOrders.Commands;

public record ApprovePurchaseOrderCommand(Guid PurchaseOrderId, Guid UserId) : IRequest<PurchaseOrderDetailDto>;

internal sealed class ApprovePurchaseOrderCommandHandler(IUnitOfWork unitOfWork, IPurchaseReadService purchaseReadService)
    : IRequestHandler<ApprovePurchaseOrderCommand, PurchaseOrderDetailDto>
{
    public async Task<PurchaseOrderDetailDto> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException("La orden de compra no existe.");

        if (order.Status != PurchaseOrderStatuses.Draft)
        {
            throw new InvalidOperationException($"Solo se puede aprobar una orden en borrador. Estado actual: {order.Status}.");
        }

        var now = DateTime.UtcNow;
        order.Status = PurchaseOrderStatuses.Approved;
        order.ApprovedBy = request.UserId;
        order.ApprovedAt = now;
        order.UpdatedAt = now;

        await unitOfWork.CompleteAsync(cancellationToken);

        return await purchaseReadService.GetOrderAsync(order.PurchaseOrderId, cancellationToken);
    }
}
