namespace Magazing.Application.DTOs.Purchases;

public record SupplierRequest(
    string Ruc,
    string BusinessName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address);

public record UpdateSupplierRequest(
    string Ruc,
    string BusinessName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);
