using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Stocks.Queries;

public record GetStockSummaryQuery(Guid WarehouseId) : IRequest<StockSummaryDto>;

internal sealed class GetStockSummaryQueryHandler(IUnitOfWork unitOfWork, IInventoryReadService inventoryReadService)
    : IRequestHandler<GetStockSummaryQuery, StockSummaryDto>
{
    public async Task<StockSummaryDto> Handle(GetStockSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<Warehouse>().AnyAsync(item => item.WarehouseId == request.WarehouseId, cancellationToken))
        {
            throw new KeyNotFoundException("El almacen no existe.");
        }

        return await inventoryReadService.GetStockSummaryAsync(request.WarehouseId, cancellationToken);
    }
}
