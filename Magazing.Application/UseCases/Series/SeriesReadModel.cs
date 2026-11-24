using System.Text.RegularExpressions;
using Magazing.Application.DTOs.Settings;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Magazing.Domain.Services.Sales;
using MediatR;

namespace Magazing.Application.UseCases.Series;

internal static class SeriesReadModel
{
    public static async Task<List<DocumentSeriesDto>> ToDtosAsync(IUnitOfWork unitOfWork, IReadOnlyList<DocumentSeries> series, CancellationToken cancellationToken)
    {
        var branchIds = series.Select(item => item.BranchId).Distinct().ToList();
        var branchNames = (await unitOfWork.Repository<Branch>().FindAsync(item => branchIds.Contains(item.BranchId), cancellationToken))
            .ToDictionary(item => item.BranchId, item => item.Name);

        return series.Select(item => new DocumentSeriesDto
        {
            DocumentSeriesId = item.DocumentSeriesId,
            BranchId = item.BranchId,
            BranchName = branchNames.GetValueOrDefault(item.BranchId, string.Empty),
            DocumentType = item.DocumentType,
            Series = item.Series,
            CurrentNumber = item.CurrentNumber,
            IsActive = item.IsActive
        }).ToList();
    }
}
