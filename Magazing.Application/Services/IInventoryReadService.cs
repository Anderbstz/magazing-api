using Magazing.Application.DTOs.Inventory;
using Magazing.Domain.Common;

namespace Magazing.Application.Services;

public interface IInventoryReadService
{
    Task<PagedResult<StockItemDto>> GetStocksAsync(
        Guid warehouseId,
        string? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<StockSummaryDto> GetStockSummaryAsync(Guid warehouseId, CancellationToken cancellationToken = default);

    Task<PagedResult<MovementDto>> GetMovementsAsync(MovementFilter filter, CancellationToken cancellationToken = default);

    Task<MovementDto> GetMovementAsync(Guid inventoryMovementId, CancellationToken cancellationToken = default);
}
