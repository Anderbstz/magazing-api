namespace Magazing.Application.DTOs.Sales;

public record SaleFilter(
    string? Status,
    string? DocumentType,
    string? Search,
    DateTime? FromUtc,
    DateTime? ToUtcExclusive,
    int Page,
    int PageSize);
