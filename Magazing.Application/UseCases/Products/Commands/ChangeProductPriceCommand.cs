using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Products.Commands;

public record ChangeProductPriceCommand(Guid ProductId, decimal Price, Guid ChangedBy) : IRequest<ProductDto>;

internal sealed class ChangeProductPriceCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeProductPriceCommand, ProductDto>
{
    public async Task<ProductDto> Handle(ChangeProductPriceCommand request, CancellationToken cancellationToken)
    {
        ProductReadModel.ValidatePrice(request.Price);

        if (!await unitOfWork.Repository<Product>().AnyAsync(item => item.ProductId == request.ProductId, cancellationToken))
        {
            throw new KeyNotFoundException("El producto no existe.");
        }

        var prices = unitOfWork.Repository<ProductPrice>();
        var currentPrices = (await prices.FindAsync(item => item.ProductId == request.ProductId && item.ValidTo == null, cancellationToken)).ToList();

        if (currentPrices.Count == 1 && currentPrices[0].Price == request.Price)
        {
            throw new InvalidOperationException("El precio indicado es igual al precio vigente.");
        }

        var now = DateTime.UtcNow;

        foreach (var currentPrice in currentPrices)
        {
            currentPrice.ValidTo = now;
            prices.Update(currentPrice);
        }

        await prices.AddAsync(new ProductPrice
        {
            ProductPriceId = Guid.NewGuid(),
            ProductId = request.ProductId,
            Price = request.Price,
            ValidFrom = now,
            CreatedBy = request.ChangedBy,
            CreatedAt = now
        }, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return await ProductReadModel.GetAsync(unitOfWork, request.ProductId, cancellationToken);
    }
}
