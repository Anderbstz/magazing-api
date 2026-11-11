using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Common;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Stocks.Queries;

public record GetStocksQuery(
    Guid WarehouseId,
    string? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<StockItemDto>>;

internal sealed class GetStocksQueryHandler(IUnitOfWork unitOfWork, IInventoryReadService inventoryReadService)
    : IRequestHandler<GetStocksQuery, PagedResult<StockItemDto>>
{
    public async Task<PagedResult<StockItemDto>> Handle(GetStocksQuery request, CancellationToken cancellationToken)
    {
        var status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim().ToUpper();
        if (status is not null && !StockStatus.IsValid(status))
        {
            throw new InvalidOperationException("El estado debe ser OK, LOW u OUT.");
        }

        if (!await unitOfWork.Repository<Warehouse>().AnyAsync(item => item.WarehouseId == request.WarehouseId, cancellationToken))
        {
            throw new KeyNotFoundException("El almacen no existe.");
        }

        return await inventoryReadService.GetStocksAsync(
            request.WarehouseId,
            status,
            request.Search,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
