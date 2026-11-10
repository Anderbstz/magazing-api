using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Inventory;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Inventory.Commands;

public record CreateAdjustmentCommand(
    Guid ProductId,
    Guid WarehouseId,
    string Type,
    decimal Quantity,
    decimal? UnitCost,
    string Reason,
    Guid UserId) : IRequest<MovementDto>;

internal sealed class CreateAdjustmentCommandHandler(
    IUnitOfWork unitOfWork,
    InventoryLedger inventoryLedger,
    IInventoryReadService inventoryReadService)
    : IRequestHandler<CreateAdjustmentCommand, MovementDto>
{
    public async Task<MovementDto> Handle(CreateAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var type = request.Type?.Trim().ToUpper();
        if (type is not (MovementTypeCodes.AdjustIn or MovementTypeCodes.AdjustOut))
        {
            throw new InvalidOperationException("El tipo de ajuste debe ser ADJUST_IN o ADJUST_OUT.");
        }

        var reason = request.Reason?.Trim() ?? string.Empty;
        if (reason.Length < 3)
        {
            throw new InvalidOperationException("El motivo del ajuste es obligatorio.");
        }

        if (type == MovementTypeCodes.AdjustOut && request.UnitCost is not null)
        {
            throw new InvalidOperationException("El costo unitario solo se indica en ajustes de ingreso.");
        }

        var movement = await inventoryLedger.RegisterAsync(new InventoryMovementRequest(
            request.ProductId,
            request.WarehouseId,
            type,
            request.Quantity,
            request.UnitCost,
            ReferenceTypes.Adjustment,
            null,
            reason,
            request.UserId), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return await inventoryReadService.GetMovementAsync(movement.InventoryMovementId, cancellationToken);
    }
}
