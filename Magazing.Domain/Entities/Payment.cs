using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid SaleId { get; set; }

    public Guid PaymentMethodId { get; set; }

    public decimal Amount { get; set; }

    public string? Reference { get; set; }

    public DateTime PaidAt { get; set; }

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;
}
