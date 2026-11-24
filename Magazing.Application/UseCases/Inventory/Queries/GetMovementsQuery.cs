using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Services.Common;
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
        var filter = MovementFilterFactory.Create(
            request.ProductId, request.WarehouseId, request.Type, request.From, request.To, request.Page, request.PageSize);

        return inventoryReadService.GetMovementsAsync(filter, cancellationToken);
    }
}
