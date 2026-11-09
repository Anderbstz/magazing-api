using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Products.Queries;

public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDto>;

internal sealed class GetProductByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return ProductReadModel.GetAsync(unitOfWork, request.ProductId, cancellationToken);
    }
}
