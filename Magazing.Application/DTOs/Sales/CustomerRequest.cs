namespace Magazing.Application.DTOs.Sales;

public record CustomerRequest(
    string DocumentType,
    string DocumentNumber,
    string FullName,
    string? Email,
    string? Phone,
    string? Address);

public record UpdateCustomerRequest(
    string DocumentType,
    string DocumentNumber,
    string FullName,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);
