using Magazing.Application.Common;
using Magazing.Application.DTOs.Sales;
using Magazing.Domain.Entities;
using Magazing.Domain.Sales;

namespace Magazing.Application.UseCases.Customers;

internal static class CustomerReadModel
{
    public static CustomerDto ToDto(Customer customer)
    {
        return new CustomerDto
        {
            CustomerId = customer.CustomerId,
            DocumentType = customer.DocumentType,
            DocumentNumber = customer.DocumentNumber,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };
    }

    public static CustomerData Normalize(
        string? documentType,
        string? documentNumber,
        string? fullName,
        string? email,
        string? phone,
        string? address)
    {
        var data = new CustomerData(
            documentType?.Trim().ToUpper() ?? string.Empty,
            documentNumber?.Trim().ToUpper() ?? string.Empty,
            fullName?.Trim() ?? string.Empty,
            Clean(email)?.ToLower(),
            Clean(phone),
            Clean(address));

        if (!DocumentTypes.IsValid(data.DocumentType))
        {
            throw new InvalidOperationException("El tipo de documento debe ser DNI, RUC, CE o PASSPORT.");
        }

        var validNumber = data.DocumentType switch
        {
            DocumentTypes.Dni => DocumentValidator.IsValidDni(data.DocumentNumber),
            DocumentTypes.Ruc => DocumentValidator.IsValidRuc(data.DocumentNumber),
            _ => data.DocumentNumber.Length is >= 6 and <= 20
        };

        if (!validNumber)
        {
            throw new InvalidOperationException($"El numero de documento no es valido para {data.DocumentType}.");
        }

        if (SalesRules.IsGenericCustomer(data.DocumentType, data.DocumentNumber))
        {
            throw new InvalidOperationException("El documento 00000000 esta reservado para CLIENTES VARIOS.");
        }

        if (string.IsNullOrWhiteSpace(data.FullName))
        {
            throw new InvalidOperationException("El nombre o razon social es obligatorio.");
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

    public sealed record CustomerData(
        string DocumentType,
        string DocumentNumber,
        string FullName,
        string? Email,
        string? Phone,
        string? Address);
}
