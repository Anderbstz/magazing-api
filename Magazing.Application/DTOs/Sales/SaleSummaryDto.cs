namespace Magazing.Application.DTOs.Sales;

public class SaleSummaryDto
{
    public Guid SaleId { get; set; }

    public string FullNumber { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string CustomerDocumentType { get; set; } = null!;

    public string CustomerDocumentNumber { get; set; } = null!;

    public string SellerName { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int ItemCount { get; set; }

    public decimal Total { get; set; }

    public List<string> PaymentMethods { get; set; } = [];

    public DateTime SoldAt { get; set; }
}
