using Magazing.Application.Common;
using Magazing.Application.DTOs.Purchases;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Purchases;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.PurchaseOrders.Commands;

public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    Guid WarehouseId,
    DateOnly? ExpectedDate,
    string? Notes,
    List<PurchaseOrderItemRequest> Items,
    Guid UserId) : IRequest<PurchaseOrderDetailDto>;

internal sealed class CreatePurchaseOrderCommandHandler(IUnitOfWork unitOfWork, IPurchaseReadService purchaseReadService)
    : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDetailDto>
{
    public async Task<PurchaseOrderDetailDto> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var orderDate = BusinessTime.Today();
        var calculated = await PurchaseOrderCalculator.CalculateAsync(
            unitOfWork,
            request.SupplierId,
            request.WarehouseId,
            orderDate,
            request.ExpectedDate,
            request.Items,
            cancellationToken);

        var orders = unitOfWork.Repository<PurchaseOrder>();
        var sequence = (await orders.GetAllAsync(cancellationToken)).Count() + 1;

        var order = new PurchaseOrder
        {
            PurchaseOrderId = Guid.NewGuid(),
            Code = $"OC-{sequence:D6}",
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            Status = PurchaseOrderStatuses.Draft,
            OrderDate = orderDate,
            ExpectedDate = request.ExpectedDate,
            Subtotal = calculated.Subtotal,
            TaxAmount = calculated.TaxAmount,
            Total = calculated.Total,
            Notes = request.Notes?.Trim(),
            CreatedBy = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await orders.AddAsync(order, cancellationToken);
        await PurchaseOrderCalculator.AddDetailsAsync(unitOfWork, order.PurchaseOrderId, calculated.Lines, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return await purchaseReadService.GetOrderAsync(order.PurchaseOrderId, cancellationToken);
    }
}
