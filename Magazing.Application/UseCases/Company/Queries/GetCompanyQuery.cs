using Magazing.Application.Services.Common;
using Magazing.Application.DTOs.Settings;
using Magazing.Domain.Repositories;
using MediatR;
using CompanyEntity = Magazing.Domain.Entities.Company;

namespace Magazing.Application.UseCases.Company.Queries;

public record GetCompanyQuery : IRequest<CompanyDto>;

internal sealed class GetCompanyQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCompanyQuery, CompanyDto>
{
    public async Task<CompanyDto> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
    {
        return CompanyReadModel.ToDto(await CompanyReadModel.GetAsync(unitOfWork, cancellationToken));
    }
}
