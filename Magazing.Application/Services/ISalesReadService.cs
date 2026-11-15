using Magazing.Application.DTOs.Sales;
using Magazing.Domain.Common;

namespace Magazing.Application.Services;

public interface ISalesReadService
{
    Task<PagedResult<SaleSummaryDto>> GetSalesAsync(SaleFilter filter, CancellationToken cancellationToken = default);

    Task<SaleDetailDto> GetSaleAsync(Guid saleId, CancellationToken cancellationToken = default);
}
