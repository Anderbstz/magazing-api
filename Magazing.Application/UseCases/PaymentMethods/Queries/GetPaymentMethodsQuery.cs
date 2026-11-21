using Magazing.Application.DTOs.Sales;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.PaymentMethods.Queries;

public record GetPaymentMethodsQuery(bool IncludeInactive = false) : IRequest<List<PaymentMethodDto>>;

internal sealed class GetPaymentMethodsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPaymentMethodsQuery, List<PaymentMethodDto>>
{
    public async Task<List<PaymentMethodDto>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        var methods = request.IncludeInactive
            ? await unitOfWork.Repository<PaymentMethod>().GetAllAsync(cancellationToken)
            : await unitOfWork.Repository<PaymentMethod>().FindAsync(item => item.IsActive, cancellationToken);

        return methods
            .OrderBy(item => item.Name)
            .Select(PaymentMethodReadModel.ToDto)
            .ToList();
    }
}
