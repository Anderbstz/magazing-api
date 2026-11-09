using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Brands.Queries;

public record GetBrandsQuery(bool? IsActive = null) : IRequest<List<BrandDto>>;

internal sealed class GetBrandsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBrandsQuery, List<BrandDto>>
{
    public async Task<List<BrandDto>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = request.IsActive is { } isActive
            ? await unitOfWork.Repository<Brand>().FindAsync(item => item.IsActive == isActive, cancellationToken)
            : await unitOfWork.Repository<Brand>().GetAllAsync(cancellationToken);

        return brands
            .OrderBy(item => item.Name)
            .Select(BrandReadModel.ToDto)
            .ToList();
    }
}
