using Magazing.Application.Common;
using Magazing.Application.DTOs.Purchases;
using Magazing.Domain.Entities;

namespace Magazing.Application.UseCases.Suppliers;

internal static class SupplierReadModel
{
    public static SupplierDto ToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            Ruc = supplier.Ruc,
            BusinessName = supplier.BusinessName,
            ContactName = supplier.ContactName,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt
        };
    }

    public static SupplierData Normalize(string? ruc, string? businessName, string? contactName, string? email, string? phone, string? address)
    {
        var data = new SupplierData(
            ruc?.Trim() ?? string.Empty,
            businessName?.Trim() ?? string.Empty,
            Clean(contactName),
            Clean(email)?.ToLower(),
            Clean(phone),
            Clean(address));

        if (!DocumentValidator.IsValidRuc(data.Ruc))
        {
            throw new InvalidOperationException("El RUC debe tener 11 digitos y empezar con 10, 15, 16, 17 o 20.");
        }

        if (string.IsNullOrWhiteSpace(data.BusinessName))
        {
            throw new InvalidOperationException("La razon social es obligatoria.");
        }

        if (data.Email is not null && !DocumentValidator.IsValidEmail(data.Email))
        {
            throw new InvalidOperationException("El correo no es valido.");
        }

        return data;
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public sealed record SupplierData(string Ruc, string BusinessName, string? ContactName, string? Email, string? Phone, string? Address);
}
