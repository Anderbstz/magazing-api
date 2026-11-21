using Magazing.Application.DTOs.Reports;
using Magazing.Application.Services;
using MediatR;

namespace Magazing.Application.UseCases.Reports.Queries;

public record GetTopProductsQuery(DateOnly From, DateOnly To, int Limit = 10) : IRequest<List<TopProductDto>>;

internal sealed class GetTopProductsQueryHandler(IReportsReadService reportsReadService)
    : IRequestHandler<GetTopProductsQuery, List<TopProductDto>>
{
    private const int MaxLimit = 50;

    public async Task<List<TopProductDto>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        var (fromUtc, toUtc) = SalesAggregator.Range(request.From, request.To);
        var lines = await reportsReadService.GetCompletedSaleLinesAsync(fromUtc, toUtc, cancellationToken);

        return SalesAggregator.TopProducts(lines, Math.Clamp(request.Limit, 1, MaxLimit));
    }
}
