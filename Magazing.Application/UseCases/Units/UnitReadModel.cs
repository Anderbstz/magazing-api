using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Entities;

namespace Magazing.Application.UseCases.Units;

internal static class UnitReadModel
{
    public static UnitDto ToDto(Unit unit)
    {
        return new UnitDto
        {
            UnitId = unit.UnitId,
            Code = unit.Code,
            Name = unit.Name,
            AllowsDecimals = unit.AllowsDecimals
        };
    }
}
