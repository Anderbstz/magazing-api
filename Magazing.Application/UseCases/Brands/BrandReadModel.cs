using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;

namespace Magazing.Application.UseCases.Brands;

internal static class BrandReadModel
{
    public static async Task<BrandDto> GetAsync(IUnitOfWork unitOfWork, Guid brandId, CancellationToken cancellationToken)
    {
        var brand = await unitOfWork.Repository<Brand>().FirstOrDefaultAsync(item => item.BrandId == brandId, cancellationToken)
            ?? throw new KeyNotFoundException("La marca no existe.");

        return ToDto(brand);
    }

    public static BrandDto ToDto(Brand brand)
    {
        return new BrandDto
        {
            BrandId = brand.BrandId,
            Name = brand.Name,
            IsActive = brand.IsActive,
            CreatedAt = brand.CreatedAt
        };
    }
}
