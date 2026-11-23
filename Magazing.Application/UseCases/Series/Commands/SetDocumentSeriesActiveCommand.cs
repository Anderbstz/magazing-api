using System.Text.RegularExpressions;
using Magazing.Application.DTOs.Settings;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Magazing.Domain.Services.Sales;
using MediatR;

namespace Magazing.Application.UseCases.Series.Commands;

public record SetDocumentSeriesActiveCommand(Guid DocumentSeriesId, bool IsActive) : IRequest<DocumentSeriesDto>;

internal sealed class SetDocumentSeriesActiveCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SetDocumentSeriesActiveCommand, DocumentSeriesDto>
{
    public async Task<DocumentSeriesDto> Handle(SetDocumentSeriesActiveCommand request, CancellationToken cancellationToken)
    {
        var seriesRepository = unitOfWork.Repository<DocumentSeries>();
        var series = await seriesRepository.GetByIdAsync(request.DocumentSeriesId, cancellationToken)
            ?? throw new KeyNotFoundException("La serie no existe.");

        if (series.IsActive && !request.IsActive
            && !await seriesRepository.AnyAsync(
                item => item.BranchId == series.BranchId
                    && item.DocumentType == series.DocumentType
                    && item.IsActive
                    && item.DocumentSeriesId != series.DocumentSeriesId,
                cancellationToken))
        {
            throw new InvalidOperationException($"La sucursal debe conservar al menos una serie activa de {series.DocumentType}.");
        }

        series.IsActive = request.IsActive;
        await unitOfWork.CompleteAsync(cancellationToken);

        return (await SeriesReadModel.ToDtosAsync(unitOfWork, [series], cancellationToken))[0];
    }
}
