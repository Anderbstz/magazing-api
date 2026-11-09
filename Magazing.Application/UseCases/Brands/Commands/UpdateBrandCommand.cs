using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Brands.Commands;

public record UpdateBrandCommand(Guid BrandId, string Name, bool IsActive) : IRequest<BrandDto>;

internal sealed class UpdateBrandCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBrandCommand, BrandDto>
{
    public async Task<BrandDto> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre de la marca es obligatorio.");
        }

        var brands = unitOfWork.Repository<Brand>();
        var brand = await brands.GetByIdAsync(request.BrandId, cancellationToken)
            ?? throw new KeyNotFoundException("La marca no existe.");

        if (await brands.AnyAsync(item => item.Name.ToLower() == name.ToLower() && item.BrandId != request.BrandId, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una marca con ese nombre.");
        }

        brand.Name = name;
        brand.IsActive = request.IsActive;
        brand.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return BrandReadModel.ToDto(brand);
    }
}
