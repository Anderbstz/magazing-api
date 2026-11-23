using Magazing.Application.DTOs.Sales;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Magazing.Domain.Services.Sales;
using MediatR;

namespace Magazing.Application.UseCases.PaymentMethods.Commands;

public record UpdatePaymentMethodCommand(Guid PaymentMethodId, string Name, bool IsActive) : IRequest<PaymentMethodDto>;

internal sealed class UpdatePaymentMethodCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePaymentMethodCommand, PaymentMethodDto>
{
    public async Task<PaymentMethodDto> Handle(UpdatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre del método de pago es obligatorio.");
        }

        var methods = unitOfWork.Repository<PaymentMethod>();
        var method = await methods.GetByIdAsync(request.PaymentMethodId, cancellationToken)
            ?? throw new KeyNotFoundException("El método de pago no existe.");

        if (!request.IsActive && method.Code == SalesRules.CashPaymentCode)
        {
            throw new InvalidOperationException("El efectivo no se puede desactivar: es el único método que admite vuelto.");
        }

        if (method.IsActive && !request.IsActive
            && !await methods.AnyAsync(item => item.IsActive && item.PaymentMethodId != request.PaymentMethodId, cancellationToken))
        {
            throw new InvalidOperationException("Debe quedar al menos un método de pago activo.");
        }

        method.Name = name;
        method.IsActive = request.IsActive;

        await unitOfWork.CompleteAsync(cancellationToken);

        return PaymentMethodReadModel.ToDto(method);
    }
}
