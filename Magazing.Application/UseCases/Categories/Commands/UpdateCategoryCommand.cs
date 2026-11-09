using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Categories.Commands;

public record UpdateCategoryCommand(Guid CategoryId, string Name, string? Description, Guid? ParentCategoryId, bool IsActive) : IRequest<CategoryDto>;

internal sealed class UpdateCategoryCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre de la categoria es obligatorio.");
        }

        var categories = unitOfWork.Repository<Category>();
        var category = await categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException("La categoria no existe.");

        if (await categories.AnyAsync(item => item.Name.ToLower() == name.ToLower() && item.CategoryId != request.CategoryId, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una categoria con ese nombre.");
        }

        await CategoryReadModel.EnsureValidParentAsync(unitOfWork, request.CategoryId, request.ParentCategoryId, cancellationToken);

        category.Name = name;
        category.Description = request.Description?.Trim();
        category.ParentCategoryId = request.ParentCategoryId;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return await CategoryReadModel.GetAsync(unitOfWork, category.CategoryId, cancellationToken);
    }
}
