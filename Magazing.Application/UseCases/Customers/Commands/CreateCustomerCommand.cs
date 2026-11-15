using Magazing.Application.DTOs.Sales;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Customers.Commands;

public record CreateCustomerCommand(
    string DocumentType,
    string DocumentNumber,
    string FullName,
    string? Email,
    string? Phone,
    string? Address) : IRequest<CustomerDto>;

internal sealed class CreateCustomerCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var data = CustomerReadModel.Normalize(
            request.DocumentType, request.DocumentNumber, request.FullName, request.Email, request.Phone, request.Address);

        var customers = unitOfWork.Repository<Customer>();
        if (await customers.AnyAsync(item => item.DocumentType == data.DocumentType && item.DocumentNumber == data.DocumentNumber, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un cliente con ese documento.");
        }

        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            DocumentType = data.DocumentType,
            DocumentNumber = data.DocumentNumber,
            FullName = data.FullName,
            Email = data.Email,
            Phone = data.Phone,
            Address = data.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await customers.AddAsync(customer, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return CustomerReadModel.ToDto(customer);
    }
}
