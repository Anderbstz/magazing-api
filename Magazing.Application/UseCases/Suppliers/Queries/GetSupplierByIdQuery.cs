using Magazing.Application.DTOs.Purchases;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Suppliers.Queries;

public record GetSupplierByIdQuery(Guid SupplierId) : IRequest<SupplierDto>;

internal sealed class GetSupplierByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSupplierByIdQuery, SupplierDto>
{
    public async Task<SupplierDto> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await unitOfWork.Repository<Supplier>().FirstOrDefaultAsync(item => item.SupplierId == request.SupplierId, cancellationToken)
            ?? throw new KeyNotFoundException("El proveedor no existe.");

        return SupplierReadModel.ToDto(supplier);
    }
}
