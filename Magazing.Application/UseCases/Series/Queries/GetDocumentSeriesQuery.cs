using System.Text.RegularExpressions;
using Magazing.Application.DTOs.Settings;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Magazing.Domain.Services.Sales;
using MediatR;

namespace Magazing.Application.UseCases.Series.Queries;

public record GetDocumentSeriesQuery : IRequest<List<DocumentSeriesDto>>;

internal sealed class GetDocumentSeriesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetDocumentSeriesQuery, List<DocumentSeriesDto>>
{
    public async Task<List<DocumentSeriesDto>> Handle(GetDocumentSeriesQuery request, CancellationToken cancellationToken)
    {
        var series = (await unitOfWork.Repository<DocumentSeries>().GetAllAsync(cancellationToken))
            .OrderBy(item => item.DocumentType)
            .ThenBy(item => item.Series)
            .ToList();

        return await SeriesReadModel.ToDtosAsync(unitOfWork, series, cancellationToken);
    }
}
