using System.Text.RegularExpressions;
using Magazing.Application.DTOs.Settings;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Magazing.Domain.Sales;
using MediatR;

namespace Magazing.Application.UseCases.Series.Commands;

public record CreateDocumentSeriesCommand(Guid BranchId, string DocumentType, string Series) : IRequest<DocumentSeriesDto>;

internal sealed partial class CreateDocumentSeriesCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateDocumentSeriesCommand, DocumentSeriesDto>
{
    public async Task<DocumentSeriesDto> Handle(CreateDocumentSeriesCommand request, CancellationToken cancellationToken)
    {
        var documentType = request.DocumentType?.Trim().ToUpper();
        var seriesCode = request.Series?.Trim().ToUpper() ?? string.Empty;

        if (!InvoiceTypes.IsValid(documentType))
        {
            throw new InvalidOperationException("El tipo debe ser BOLETA o FACTURA.");
        }

        var prefix = documentType == InvoiceTypes.Boleta ? 'B' : 'F';
        if (!SeriesRegex().IsMatch(seriesCode) || seriesCode[0] != prefix)
        {
            throw new InvalidOperationException($"La serie de {documentType} debe tener 4 caracteres y empezar con {prefix} (ej. {prefix}002).");
        }

        if (!await unitOfWork.Repository<Branch>().AnyAsync(item => item.BranchId == request.BranchId && item.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("La sucursal no existe o esta inactiva.");
        }

        var seriesRepository = unitOfWork.Repository<DocumentSeries>();
        if (await seriesRepository.AnyAsync(item => item.Series == seriesCode, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe esa serie.");
        }

        var series = new DocumentSeries
        {
            DocumentSeriesId = Guid.NewGuid(),
            BranchId = request.BranchId,
            DocumentType = documentType!,
            Series = seriesCode,
            CurrentNumber = 0,
            IsActive = true
        };

        await seriesRepository.AddAsync(series, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return (await SeriesReadModel.ToDtosAsync(unitOfWork, [series], cancellationToken))[0];
    }

    [GeneratedRegex("^[BF][0-9A-Z]{3}$")]
    private static partial Regex SeriesRegex();
}
