using Magazing.Application.DTOs.Sales;
using Magazing.Application.Services;
using Magazing.Application.Services.Common;
using Magazing.Application.UseCases.Products;
using Magazing.Application.UseCases.Reports;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Sales.Queries;

public record GetPosTopProductsQuery(Guid WarehouseId, int Limit = 12) : IRequest<List<PosProductDto>>;

internal sealed class GetPosTopProductsQueryHandler(IUnitOfWork unitOfWork, IReportsReadService reportsReadService)
    : IRequestHandler<GetPosTopProductsQuery, List<PosProductDto>>
{
    private const int MaxLimit = 24;
    private const int WindowDays = 30;

    public async Task<List<PosProductDto>> Handle(GetPosTopProductsQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit, 1, MaxLimit);
        var today = BusinessTime.Today();
        var (fromUtc, toUtc) = SalesAggregator.Range(today.AddDays(1 - WindowDays), today);

        var lines = await reportsReadService.GetCompletedSaleLinesAsync(fromUtc, toUtc, cancellationToken);
        var sold = SalesAggregator.TopProducts(lines, MaxLimit * 2).ToDictionary(item => item.ProductId, item => item.Quantity);
        var soldIds = sold.Keys.ToList();

        var products = unitOfWork.Repository<Product>();
        var candidates = (await products.FindAsync(item => soldIds.Contains(item.ProductId) && item.IsActive, cancellationToken)).ToList();

        if (candidates.Count < limit)
        {
            var filler = await products.GetPagedAsync(
                1,
                limit * 2,
                item => item.IsActive && !soldIds.Contains(item.ProductId),
                item => item.Name,
                cancellationToken: cancellationToken);
            candidates.AddRange(filler.Items);
        }

        var dtos = (await ProductReadModel.ToDtosAsync(unitOfWork, candidates, cancellationToken))
            .Where(item => item.CurrentPrice is not null)
            .OrderByDescending(item => sold.GetValueOrDefault(item.ProductId))
            .ThenBy(item => item.Name)
            .Take(limit)
            .ToList();

        var ids = dtos.Select(item => item.ProductId).ToList();
        var stock = (await unitOfWork.Repository<Stock>()
                .FindAsync(item => item.WarehouseId == request.WarehouseId && ids.Contains(item.ProductId), cancellationToken))
            .ToDictionary(item => item.ProductId, item => item.Quantity);

        return dtos.Select(item => new PosProductDto
        {
            Product = item,
            QuantitySold = sold.GetValueOrDefault(item.ProductId),
            Stock = stock.GetValueOrDefault(item.ProductId)
        }).ToList();
    }
}
