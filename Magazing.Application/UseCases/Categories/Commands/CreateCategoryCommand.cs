using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Categories.Commands;

public record CreateCategoryCommand(string Name, string? Description, Guid? ParentCategoryId) : IRequest<CategoryDto>;

internal sealed class CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre de la categoria es obligatorio.");
        }

        var categories = unitOfWork.Repository<Category>();
        if (await categories.AnyAsync(item => item.Name.ToLower() == name.ToLower(), cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una categoria con ese nombre.");
        }

        await CategoryReadModel.EnsureValidParentAsync(unitOfWork, null, request.ParentCategoryId, cancellationToken);

        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            ParentCategoryId = request.ParentCategoryId,
            Name = name,
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await categories.AddAsync(category, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return await CategoryReadModel.GetAsync(unitOfWork, category.CategoryId, cancellationToken);
    }
}
