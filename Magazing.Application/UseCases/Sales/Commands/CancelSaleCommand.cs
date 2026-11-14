using Magazing.Application.DTOs.Sales;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Inventory;
using Magazing.Domain.Repositories;
using Magazing.Domain.Sales;
using MediatR;

namespace Magazing.Application.UseCases.Sales.Commands;

public record CancelSaleCommand(Guid SaleId, string Reason, Guid UserId) : IRequest<SaleDetailDto>;

internal sealed class CancelSaleCommandHandler(
    IUnitOfWork unitOfWork,
    InventoryLedger inventoryLedger,
    ISalesReadService salesReadService)
    : IRequestHandler<CancelSaleCommand, SaleDetailDto>
{
    private const int MinReasonLength = 5;

    public async Task<SaleDetailDto> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var reason = request.Reason?.Trim() ?? string.Empty;
        if (reason.Length < MinReasonLength)
        {
            throw new InvalidOperationException($"El motivo de anulacion debe tener al menos {MinReasonLength} caracteres.");
        }

        var sale = await unitOfWork.Repository<Sale>().GetByIdAsync(request.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException("La venta no existe.");

        if (sale.Status != SaleStatuses.Completed)
        {
            throw new InvalidOperationException($"Solo se puede anular una venta completada. Estado actual: {sale.Status}.");
        }

        var invoices = unitOfWork.Repository<Invoice>();
        var invoice = await invoices.FirstOrDefaultAsync(item => item.SaleId == sale.SaleId, cancellationToken);
        var notes = $"Anulacion de {invoice?.FullNumber ?? "venta"}: {reason}";

        var details = await unitOfWork.Repository<SaleDetail>().FindAsync(item => item.SaleId == sale.SaleId, cancellationToken);
        foreach (var detail in details)
        {
            await inventoryLedger.RegisterAsync(new InventoryMovementRequest(
                detail.ProductId,
                sale.WarehouseId,
                MovementTypeCodes.SaleCancelIn,
                detail.Quantity,
                detail.UnitCost,
                ReferenceTypes.Sale,
                sale.SaleId,
                notes,
                request.UserId), cancellationToken);
        }

        if (invoice is not null)
        {
            invoice.Status = InvoiceStatuses.Cancelled;
            invoices.Update(invoice);
        }

        sale.Status = SaleStatuses.Cancelled;
        sale.CancelledAt = DateTime.UtcNow;
        sale.CancelReason = reason;
        sale.CancelledBy = request.UserId;

        await unitOfWork.CompleteAsync(cancellationToken);

        return await salesReadService.GetSaleAsync(sale.SaleId, cancellationToken);
    }
}
