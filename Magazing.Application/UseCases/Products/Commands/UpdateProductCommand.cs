using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Products.Commands;

public record UpdateProductCommand(
    Guid ProductId,
    string? Barcode,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid? BrandId,
    Guid UnitId,
    bool IsTaxable,
    decimal MinStock,
    decimal? MaxStock,
    string? ImageUrl,
    bool IsActive) : IRequest<ProductDto>;

internal sealed class UpdateProductCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim();
        var name = request.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre del producto es obligatorio.");
        }

        var products = unitOfWork.Repository<Product>();
        var product = await products.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("El producto no existe.");

        await ProductReadModel.ValidateAsync(unitOfWork, request.CategoryId, request.BrandId, request.UnitId, request.MinStock, request.MaxStock, cancellationToken);

        if (barcode is not null
            && await products.AnyAsync(item => item.Barcode == barcode && item.ProductId != request.ProductId, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un producto con ese codigo de barras.");
        }

        product.Barcode = barcode;
        product.Name = name;
        product.Description = request.Description?.Trim();
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.UnitId = request.UnitId;
        product.IsTaxable = request.IsTaxable;
        product.MinStock = request.MinStock;
        product.MaxStock = request.MaxStock;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return await ProductReadModel.GetAsync(unitOfWork, product.ProductId, cancellationToken);
    }
}
