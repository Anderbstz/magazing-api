using Magazing.Application.DTOs.Sales;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Customers.Queries;

public record GetCustomerByIdQuery(Guid CustomerId) : IRequest<CustomerDto>;

internal sealed class GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await unitOfWork.Repository<Customer>().FirstOrDefaultAsync(item => item.CustomerId == request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("El cliente no existe.");

        return CustomerReadModel.ToDto(customer);
    }
}
