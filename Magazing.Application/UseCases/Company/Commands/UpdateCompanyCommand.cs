using Magazing.Application.Services.Common;
using Magazing.Application.DTOs.Settings;
using Magazing.Domain.Repositories;
using MediatR;
using CompanyEntity = Magazing.Domain.Entities.Company;

namespace Magazing.Application.UseCases.Company.Commands;

public record UpdateCompanyCommand(
    string Ruc,
    string BusinessName,
    string? TradeName,
    string? Address,
    string? Phone,
    string? Email,
    string? LogoUrl) : IRequest<CompanyDto>;

internal sealed class UpdateCompanyCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var ruc = request.Ruc?.Trim() ?? string.Empty;
        var businessName = request.BusinessName?.Trim() ?? string.Empty;
        var email = Clean(request.Email)?.ToLower();

        if (!DocumentValidator.IsValidRuc(ruc))
        {
            throw new InvalidOperationException("El RUC debe tener 11 digitos y empezar con 10, 15, 16, 17 o 20.");
        }

        if (string.IsNullOrWhiteSpace(businessName))
        {
            throw new InvalidOperationException("La razón social es obligatoria.");
        }

        if (email is not null && !DocumentValidator.IsValidEmail(email))
        {
            throw new InvalidOperationException("El correo no es válido.");
        }

        var tracked = await unitOfWork.Repository<CompanyEntity>().GetByIdAsync(
                (await CompanyReadModel.GetAsync(unitOfWork, cancellationToken)).CompanyId,
                cancellationToken)
            ?? throw new KeyNotFoundException("La empresa no está configurada.");

        tracked.Ruc = ruc;
        tracked.BusinessName = businessName;
        tracked.TradeName = Clean(request.TradeName);
        tracked.Address = Clean(request.Address);
        tracked.Phone = Clean(request.Phone);
        tracked.Email = email;
        tracked.LogoUrl = Clean(request.LogoUrl);
        tracked.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return CompanyReadModel.ToDto(tracked);
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
