using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Products.Queries;

public record GetProductPriceHistoryQuery(Guid ProductId) : IRequest<List<ProductPriceDto>>;

internal sealed class GetProductPriceHistoryQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProductPriceHistoryQuery, List<ProductPriceDto>>
{
    public async Task<List<ProductPriceDto>> Handle(GetProductPriceHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<Product>().AnyAsync(item => item.ProductId == request.ProductId, cancellationToken))
        {
            throw new KeyNotFoundException("El producto no existe.");
        }

        var prices = await unitOfWork.Repository<ProductPrice>().FindAsync(item => item.ProductId == request.ProductId, cancellationToken);

        return prices
            .OrderByDescending(item => item.ValidFrom)
            .Select(item => new ProductPriceDto
            {
                ProductPriceId = item.ProductPriceId,
                Price = item.Price,
                ValidFrom = item.ValidFrom,
                ValidTo = item.ValidTo,
                IsCurrent = item.ValidTo == null,
                CreatedBy = item.CreatedBy
            })
            .ToList();
    }
}
