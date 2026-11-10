using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Categories.Queries;

public record GetCategoriesQuery(bool? IsActive = null) : IRequest<List<CategoryDto>>;

internal sealed class GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = (await unitOfWork.Repository<Category>().GetAllAsync(cancellationToken)).ToList();
        var names = categories.ToDictionary(item => item.CategoryId, item => item.Name);

        return categories
            .Where(item => request.IsActive is null || item.IsActive == request.IsActive)
            .OrderBy(item => item.Name)
            .Select(item => CategoryReadModel.ToDto(
                item,
                item.ParentCategoryId is { } parentId && names.TryGetValue(parentId, out var parentName) ? parentName : null))
            .ToList();
    }
}
