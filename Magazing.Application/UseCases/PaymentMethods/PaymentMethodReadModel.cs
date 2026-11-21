using Magazing.Application.DTOs.Sales;
using Magazing.Domain.Entities;

namespace Magazing.Application.UseCases.PaymentMethods;

internal static class PaymentMethodReadModel
{
    public static PaymentMethodDto ToDto(PaymentMethod method)
    {
        return new PaymentMethodDto
        {
            PaymentMethodId = method.PaymentMethodId,
            Code = method.Code,
            Name = method.Name,
            IsActive = method.IsActive
        };
    }
}
