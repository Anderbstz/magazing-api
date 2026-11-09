using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Categories.Commands;

public record DeleteCategoryCommand(Guid CategoryId) : IRequest;

internal sealed class DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var categories = unitOfWork.Repository<Category>();
        var category = await categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException("La categoria no existe.");

        if (await unitOfWork.Repository<Product>().AnyAsync(item => item.CategoryId == request.CategoryId, cancellationToken))
        {
            throw new InvalidOperationException("La categoria tiene productos asociados. Desactivela en lugar de eliminarla.");
        }

        if (await categories.AnyAsync(item => item.ParentCategoryId == request.CategoryId, cancellationToken))
        {
            throw new InvalidOperationException("La categoria tiene subcategorias.");
        }

        categories.Delete(category);
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}
