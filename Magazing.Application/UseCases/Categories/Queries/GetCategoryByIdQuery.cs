using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Categories.Queries;

public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<CategoryDto>;

internal sealed class GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    public Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        return CategoryReadModel.GetAsync(unitOfWork, request.CategoryId, cancellationToken);
    }
}
