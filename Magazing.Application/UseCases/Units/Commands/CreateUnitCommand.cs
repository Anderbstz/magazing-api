using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;
using Unit = Magazing.Domain.Entities.Unit;

namespace Magazing.Application.UseCases.Units.Commands;

public record CreateUnitCommand(string Code, string Name, bool AllowsDecimals) : IRequest<UnitDto>;

internal sealed class CreateUnitCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUnitCommand, UnitDto>
{
    private const int MaxCodeLength = 10;

    public async Task<UnitDto> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code?.Trim().ToUpper() ?? string.Empty;
        var name = request.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(code) || code.Length > MaxCodeLength)
        {
            throw new InvalidOperationException($"El codigo es obligatorio y debe tener como maximo {MaxCodeLength} caracteres.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre de la unidad es obligatorio.");
        }

        var units = unitOfWork.Repository<Unit>();
        if (await units.AnyAsync(item => item.Code == code, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una unidad con ese codigo.");
        }

        var unit = new Unit
        {
            UnitId = Guid.NewGuid(),
            Code = code,
            Name = name,
            AllowsDecimals = request.AllowsDecimals
        };

        await units.AddAsync(unit, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return UnitReadModel.ToDto(unit);
    }
}
