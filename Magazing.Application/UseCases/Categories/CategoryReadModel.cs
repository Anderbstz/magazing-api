using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;

namespace Magazing.Application.UseCases.Categories;

internal static class CategoryReadModel
{
    public static async Task<CategoryDto> GetAsync(IUnitOfWork unitOfWork, Guid categoryId, CancellationToken cancellationToken)
    {
        var categories = unitOfWork.Repository<Category>();
        var category = await categories.FirstOrDefaultAsync(item => item.CategoryId == categoryId, cancellationToken)
            ?? throw new KeyNotFoundException("La categoria no existe.");

        string? parentName = null;
        if (category.ParentCategoryId is { } parentId)
        {
            parentName = (await categories.FirstOrDefaultAsync(item => item.CategoryId == parentId, cancellationToken))?.Name;
        }

        return ToDto(category, parentName);
    }

    public static CategoryDto ToDto(Category category, string? parentName)
    {
        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = parentName,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }

    public static async Task EnsureValidParentAsync(IUnitOfWork unitOfWork, Guid? categoryId, Guid? parentCategoryId, CancellationToken cancellationToken)
    {
        if (parentCategoryId is null)
        {
            return;
        }

        var all = (await unitOfWork.Repository<Category>().GetAllAsync(cancellationToken))
            .ToDictionary(item => item.CategoryId);

        if (!all.ContainsKey(parentCategoryId.Value))
        {
            throw new InvalidOperationException("La categoria padre no existe.");
        }

        if (categoryId is null)
        {
            return;
        }

        var currentId = parentCategoryId;
        while (currentId is not null)
        {
            if (currentId == categoryId)
            {
                throw new InvalidOperationException("Una categoria no puede ser padre de si misma ni de sus ancestros.");
            }

            currentId = all.TryGetValue(currentId.Value, out var current) ? current.ParentCategoryId : null;
        }
    }
}
