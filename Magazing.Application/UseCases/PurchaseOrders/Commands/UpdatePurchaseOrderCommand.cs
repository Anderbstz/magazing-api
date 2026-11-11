using Magazing.Application.DTOs.Purchases;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Purchases;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.PurchaseOrders.Commands;

public record UpdatePurchaseOrderCommand(
    Guid PurchaseOrderId,
    Guid SupplierId,
    Guid WarehouseId,
    DateOnly? ExpectedDate,
    string? Notes,
    List<PurchaseOrderItemRequest> Items) : IRequest<PurchaseOrderDetailDto>;

internal sealed class UpdatePurchaseOrderCommandHandler(IUnitOfWork unitOfWork, IPurchaseReadService purchaseReadService)
    : IRequestHandler<UpdatePurchaseOrderCommand, PurchaseOrderDetailDto>
{
    public async Task<PurchaseOrderDetailDto> Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException("La orden de compra no existe.");

        if (order.Status != PurchaseOrderStatuses.Draft)
        {
            throw new InvalidOperationException($"Solo se puede editar una orden en borrador. Estado actual: {order.Status}.");
        }

        var calculated = await PurchaseOrderCalculator.CalculateAsync(
            unitOfWork,
            request.SupplierId,
            request.WarehouseId,
            order.OrderDate,
            request.ExpectedDate,
            request.Items,
            cancellationToken);

        var details = unitOfWork.Repository<PurchaseOrderDetail>();
        var currentDetails = (await details.FindAsync(item => item.PurchaseOrderId == order.PurchaseOrderId, cancellationToken))
            .ToDictionary(item => item.ProductId);
        var newLines = calculated.Lines.ToDictionary(line => line.ProductId);

        foreach (var detail in currentDetails.Values)
        {
            if (newLines.TryGetValue(detail.ProductId, out var line))
            {
                detail.Quantity = line.Quantity;
                detail.UnitCost = line.UnitCost;
                detail.Subtotal = line.Subtotal;
                details.Update(detail);
            }
            else
            {
                details.Delete(detail);
            }
        }

        order.SupplierId = request.SupplierId;
        order.WarehouseId = request.WarehouseId;
        order.ExpectedDate = request.ExpectedDate;
        order.Notes = request.Notes?.Trim();
        order.Subtotal = calculated.Subtotal;
        order.TaxAmount = calculated.TaxAmount;
        order.Total = calculated.Total;
        order.UpdatedAt = DateTime.UtcNow;

        await PurchaseOrderCalculator.AddDetailsAsync(
            unitOfWork,
            order.PurchaseOrderId,
            calculated.Lines.Where(line => !currentDetails.ContainsKey(line.ProductId)),
            cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return await purchaseReadService.GetOrderAsync(order.PurchaseOrderId, cancellationToken);
    }
}
