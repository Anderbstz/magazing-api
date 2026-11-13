using Magazing.Application.DTOs.Sales;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.PaymentMethods.Queries;

public record GetPaymentMethodsQuery : IRequest<List<PaymentMethodDto>>;

internal sealed class GetPaymentMethodsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPaymentMethodsQuery, List<PaymentMethodDto>>
{
    public async Task<List<PaymentMethodDto>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        var methods = await unitOfWork.Repository<PaymentMethod>().GetAllAsync(cancellationToken);

        return methods
            .OrderBy(item => item.Name)
            .Select(item => new PaymentMethodDto
            {
                PaymentMethodId = item.PaymentMethodId,
                Code = item.Code,
                Name = item.Name,
                IsActive = item.IsActive
            })
            .ToList();
    }
}
