using Magazing.Application.DTOs.Purchases;
using Magazing.Domain.Common;

namespace Magazing.Application.Services;

public interface IPurchaseReadService
{
    Task<PagedResult<PurchaseOrderSummaryDto>> GetOrdersAsync(
        string? status,
        Guid? supplierId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PurchaseOrderDetailDto> GetOrderAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default);
}
