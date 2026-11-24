using Magazing.Application.Services.Common;
using Magazing.Application.DTOs.Settings;
using Magazing.Domain.Repositories;
using MediatR;
using CompanyEntity = Magazing.Domain.Entities.Company;

namespace Magazing.Application.UseCases.Company;

internal static class CompanyReadModel
{
    public static async Task<CompanyEntity> GetAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        return (await unitOfWork.Repository<CompanyEntity>().GetAllAsync(cancellationToken))
            .OrderBy(item => item.CreatedAt)
            .FirstOrDefault()
            ?? throw new KeyNotFoundException("La empresa no está configurada.");
    }

    public static CompanyDto ToDto(CompanyEntity company)
    {
        return new CompanyDto
        {
            CompanyId = company.CompanyId,
            Ruc = company.Ruc,
            BusinessName = company.BusinessName,
            TradeName = company.TradeName,
            Address = company.Address,
            Phone = company.Phone,
            Email = company.Email,
            LogoUrl = company.LogoUrl
        };
    }
}
