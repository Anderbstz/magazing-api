namespace Magazing.Application.DTOs.Sales;

public record CreateSaleRequest(
    Guid WarehouseId,
    Guid CustomerId,
    string DocumentType,
    string? Notes,
    List<SaleItemRequest> Items,
    List<SalePaymentRequest> Payments);

public record SaleItemRequest(Guid ProductId, decimal Quantity, decimal Discount);

public record SalePaymentRequest(Guid PaymentMethodId, decimal Amount, string? Reference);

public record CancelSaleRequest(string Reason);
