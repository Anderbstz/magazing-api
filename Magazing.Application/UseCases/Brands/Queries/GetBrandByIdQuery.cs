using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Brands.Queries;

public record GetBrandByIdQuery(Guid BrandId) : IRequest<BrandDto>;

internal sealed class GetBrandByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBrandByIdQuery, BrandDto>
{
    public Task<BrandDto> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        return BrandReadModel.GetAsync(unitOfWork, request.BrandId, cancellationToken);
    }
}
