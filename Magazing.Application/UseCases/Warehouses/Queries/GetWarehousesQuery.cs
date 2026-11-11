using Magazing.Application.DTOs.Inventory;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Warehouses.Queries;

public record GetWarehousesQuery(bool? IsActive = null) : IRequest<List<WarehouseDto>>;

internal sealed class GetWarehousesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetWarehousesQuery, List<WarehouseDto>>
{
    public async Task<List<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = request.IsActive is { } isActive
            ? await unitOfWork.Repository<Warehouse>().FindAsync(item => item.IsActive == isActive, cancellationToken)
            : await unitOfWork.Repository<Warehouse>().GetAllAsync(cancellationToken);

        return warehouses
            .OrderBy(item => item.Code)
            .Select(WarehouseReadModel.ToDto)
            .ToList();
    }
}
