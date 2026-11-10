using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;
using Unit = Magazing.Domain.Entities.Unit;

namespace Magazing.Application.UseCases.Units.Queries;

public record GetUnitsQuery : IRequest<List<UnitDto>>;

internal sealed class GetUnitsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUnitsQuery, List<UnitDto>>
{
    public async Task<List<UnitDto>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
    {
        var units = await unitOfWork.Repository<Unit>().GetAllAsync(cancellationToken);

        return units
            .OrderBy(item => item.Name)
            .Select(UnitReadModel.ToDto)
            .ToList();
    }
}
