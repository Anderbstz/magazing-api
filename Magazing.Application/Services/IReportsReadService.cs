using Magazing.Application.DTOs.Reports;

namespace Magazing.Application.Services;

public interface IReportsReadService
{
    Task<List<SaleLine>> GetCompletedSaleLinesAsync(DateTime fromUtc, DateTime toUtcExclusive, CancellationToken cancellationToken = default);

    Task<OperationalCounts> GetOperationalCountsAsync(CancellationToken cancellationToken = default);
}
