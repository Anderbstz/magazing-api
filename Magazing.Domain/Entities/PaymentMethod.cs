using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class PaymentMethod
{
    public Guid PaymentMethodId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
