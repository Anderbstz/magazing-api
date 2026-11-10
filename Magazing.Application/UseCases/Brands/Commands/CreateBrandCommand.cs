using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Brands.Commands;

public record CreateBrandCommand(string Name) : IRequest<BrandDto>;

internal sealed class CreateBrandCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBrandCommand, BrandDto>
{
    public async Task<BrandDto> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre de la marca es obligatorio.");
        }

        var brands = unitOfWork.Repository<Brand>();
        if (await brands.AnyAsync(item => item.Name.ToLower() == name.ToLower(), cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una marca con ese nombre.");
        }

        var brand = new Brand
        {
            BrandId = Guid.NewGuid(),
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await brands.AddAsync(brand, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return BrandReadModel.ToDto(brand);
    }
}
