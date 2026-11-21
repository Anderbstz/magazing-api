using Magazing.Application.Common;
using Magazing.Application.DTOs.Inventory;
using Magazing.Domain.Inventory;

namespace Magazing.Application.UseCases.Inventory;

internal static class MovementFilterFactory
{
    public static MovementFilter Create(
        Guid? productId,
        Guid? warehouseId,
        string? type,
        DateOnly? from,
        DateOnly? to,
        int page,
        int pageSize)
    {
        var normalizedType = string.IsNullOrWhiteSpace(type) ? null : type.Trim().ToUpper();
        if (normalizedType is not null && !MovementTypeCodes.All.Contains(normalizedType))
        {
            throw new InvalidOperationException("El tipo de movimiento no es valido.");
        }

        if (from is { } fromDate && to is { } toDate && fromDate > toDate)
        {
            throw new InvalidOperationException("La fecha inicial no puede ser mayor que la fecha final.");
        }

        return new MovementFilter(
            productId,
            warehouseId,
            normalizedType,
            from is { } start ? BusinessTime.StartOfDayUtc(start) : null,
            to is { } end ? BusinessTime.StartOfNextDayUtc(end) : null,
            page,
            pageSize);
    }
}
