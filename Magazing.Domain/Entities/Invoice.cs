using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Invoice
{
    public Guid InvoiceId { get; set; }

    public Guid SaleId { get; set; }

    public Guid DocumentSeriesId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string Series { get; set; } = null!;

    public int Number { get; set; }

    public string? FullNumber { get; set; }

    public string CustomerDocumentType { get; set; } = null!;

    public string CustomerDocumentNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; } = null!;

    public string? PdfUrl { get; set; }

    public DateTime IssuedAt { get; set; }

    public DateTime? EmailedAt { get; set; }

    public virtual DocumentSeries DocumentSeries { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;
}
