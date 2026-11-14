using Magazing.Application.DTOs.Sales;
using Magazing.Application.Services;
using MediatR;

namespace Magazing.Application.UseCases.Sales.Queries;

public record GetSaleByIdQuery(Guid SaleId) : IRequest<SaleDetailDto>;

internal sealed class GetSaleByIdQueryHandler(ISalesReadService salesReadService)
    : IRequestHandler<GetSaleByIdQuery, SaleDetailDto>
{
    public Task<SaleDetailDto> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        return salesReadService.GetSaleAsync(request.SaleId, cancellationToken);
    }
}
