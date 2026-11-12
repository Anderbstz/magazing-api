using Magazing.Application.DTOs.Purchases;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Suppliers.Commands;

public record UpdateSupplierCommand(
    Guid SupplierId,
    string Ruc,
    string BusinessName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive) : IRequest<SupplierDto>;

internal sealed class UpdateSupplierCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSupplierCommand, SupplierDto>
{
    public async Task<SupplierDto> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var data = SupplierReadModel.Normalize(request.Ruc, request.BusinessName, request.ContactName, request.Email, request.Phone, request.Address);

        var suppliers = unitOfWork.Repository<Supplier>();
        var supplier = await suppliers.GetByIdAsync(request.SupplierId, cancellationToken)
            ?? throw new KeyNotFoundException("El proveedor no existe.");

        if (await suppliers.AnyAsync(item => item.Ruc == data.Ruc && item.SupplierId != request.SupplierId, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un proveedor con ese RUC.");
        }

        supplier.Ruc = data.Ruc;
        supplier.BusinessName = data.BusinessName;
        supplier.ContactName = data.ContactName;
        supplier.Email = data.Email;
        supplier.Phone = data.Phone;
        supplier.Address = data.Address;
        supplier.IsActive = request.IsActive;
        supplier.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return SupplierReadModel.ToDto(supplier);
    }
}
