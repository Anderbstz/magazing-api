namespace Magazing.Application.DTOs.Sales;

public class PaymentMethodDto
{
    public Guid PaymentMethodId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }
}
