using Magazing.Application.DTOs.Purchases;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Inventory;
using Magazing.Domain.Purchases;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.PurchaseOrders.Commands;

public record ReceivePurchaseOrderCommand(Guid PurchaseOrderId, Guid UserId) : IRequest<PurchaseOrderDetailDto>;

internal sealed class ReceivePurchaseOrderCommandHandler(
    IUnitOfWork unitOfWork,
    InventoryLedger inventoryLedger,
    IPurchaseReadService purchaseReadService)
    : IRequestHandler<ReceivePurchaseOrderCommand, PurchaseOrderDetailDto>
{
    public async Task<PurchaseOrderDetailDto> Handle(ReceivePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException("La orden de compra no existe.");

        if (order.Status != PurchaseOrderStatuses.Approved)
        {
            throw new InvalidOperationException($"Solo se puede recibir una orden aprobada. Estado actual: {order.Status}.");
        }

        var details = await unitOfWork.Repository<PurchaseOrderDetail>().FindAsync(item => item.PurchaseOrderId == order.PurchaseOrderId, cancellationToken);
        var notes = $"Recepcion de {order.Code}";

        foreach (var detail in details)
        {
            await inventoryLedger.RegisterAsync(new InventoryMovementRequest(
                detail.ProductId,
                order.WarehouseId,
                MovementTypeCodes.PurchaseIn,
                detail.Quantity,
                detail.UnitCost,
                ReferenceTypes.PurchaseOrder,
                order.PurchaseOrderId,
                notes,
                request.UserId), cancellationToken);
        }

        var now = DateTime.UtcNow;
        order.Status = PurchaseOrderStatuses.Received;
        order.ReceivedAt = now;
        order.UpdatedAt = now;

        await unitOfWork.CompleteAsync(cancellationToken);

        return await purchaseReadService.GetOrderAsync(order.PurchaseOrderId, cancellationToken);
    }
}
