using Magazing.Application.Common;
using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Common;
using Magazing.Domain.Inventory;
using MediatR;

namespace Magazing.Application.UseCases.Inventory.Queries;

public record GetMovementsQuery(
    Guid? ProductId = null,
    Guid? WarehouseId = null,
    string? Type = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<MovementDto>>;

internal sealed class GetMovementsQueryHandler(IInventoryReadService inventoryReadService)
    : IRequestHandler<GetMovementsQuery, PagedResult<MovementDto>>
{
    public Task<PagedResult<MovementDto>> Handle(GetMovementsQuery request, CancellationToken cancellationToken)
    {
        var type = string.IsNullOrWhiteSpace(request.Type) ? null : request.Type.Trim().ToUpper();
        if (type is not null && !MovementTypeCodes.All.Contains(type))
        {
            throw new InvalidOperationException("El tipo de movimiento no es valido.");
        }

        if (request.From is { } from && request.To is { } to && from > to)
        {
            throw new InvalidOperationException("La fecha inicial no puede ser mayor que la fecha final.");
        }

        var filter = new MovementFilter(
            request.ProductId,
            request.WarehouseId,
            type,
            request.From is { } fromDate ? BusinessTime.StartOfDayUtc(fromDate) : null,
            request.To is { } toDate ? BusinessTime.StartOfNextDayUtc(toDate) : null,
            request.Page,
            request.PageSize);

        return inventoryReadService.GetMovementsAsync(filter, cancellationToken);
    }
}
