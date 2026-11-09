using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;
using Unit = Magazing.Domain.Entities.Unit;

namespace Magazing.Application.UseCases.Units.Commands;

public record UpdateUnitCommand(Guid UnitId, string Name, bool AllowsDecimals) : IRequest<UnitDto>;

internal sealed class UpdateUnitCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUnitCommand, UnitDto>
{
    public async Task<UnitDto> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre de la unidad es obligatorio.");
        }

        var unit = await unitOfWork.Repository<Unit>().GetByIdAsync(request.UnitId, cancellationToken)
            ?? throw new KeyNotFoundException("La unidad no existe.");

        unit.Name = name;
        unit.AllowsDecimals = request.AllowsDecimals;

        await unitOfWork.CompleteAsync(cancellationToken);

        return UnitReadModel.ToDto(unit);
    }
}
