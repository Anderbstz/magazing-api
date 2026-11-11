using Magazing.Application.DTOs.Inventory;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Warehouses.Queries;

public record GetWarehouseByIdQuery(Guid WarehouseId) : IRequest<WarehouseDto>;

internal sealed class GetWarehouseByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto>
{
    public async Task<WarehouseDto> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        return WarehouseReadModel.ToDto(await WarehouseReadModel.GetEntityAsync(unitOfWork, request.WarehouseId, cancellationToken));
    }
}
