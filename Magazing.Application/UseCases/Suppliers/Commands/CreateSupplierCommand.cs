using Magazing.Application.DTOs.Purchases;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Suppliers.Commands;

public record CreateSupplierCommand(
    string Ruc,
    string BusinessName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address) : IRequest<SupplierDto>;

internal sealed class CreateSupplierCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSupplierCommand, SupplierDto>
{
    public async Task<SupplierDto> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var data = SupplierReadModel.Normalize(request.Ruc, request.BusinessName, request.ContactName, request.Email, request.Phone, request.Address);

        var suppliers = unitOfWork.Repository<Supplier>();
        if (await suppliers.AnyAsync(item => item.Ruc == data.Ruc, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un proveedor con ese RUC.");
        }

        var supplier = new Supplier
        {
            SupplierId = Guid.NewGuid(),
            Ruc = data.Ruc,
            BusinessName = data.BusinessName,
            ContactName = data.ContactName,
            Email = data.Email,
            Phone = data.Phone,
            Address = data.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await suppliers.AddAsync(supplier, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return SupplierReadModel.ToDto(supplier);
    }
}
