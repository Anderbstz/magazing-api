using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Brands.Commands;

public record DeleteBrandCommand(Guid BrandId) : IRequest;

internal sealed class DeleteBrandCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBrandCommand>
{
    public async Task Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brands = unitOfWork.Repository<Brand>();
        var brand = await brands.GetByIdAsync(request.BrandId, cancellationToken)
            ?? throw new KeyNotFoundException("La marca no existe.");

        if (await unitOfWork.Repository<Product>().AnyAsync(item => item.BrandId == request.BrandId, cancellationToken))
        {
            throw new InvalidOperationException("La marca tiene productos asociados. Desactivela en lugar de eliminarla.");
        }

        brands.Delete(brand);
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}
