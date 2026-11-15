using Magazing.Application.DTOs.Sales;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Magazing.Domain.Sales;
using MediatR;

namespace Magazing.Application.UseCases.Customers.Commands;

public record UpdateCustomerCommand(
    Guid CustomerId,
    string DocumentType,
    string DocumentNumber,
    string FullName,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive) : IRequest<CustomerDto>;

internal sealed class UpdateCustomerCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customers = unitOfWork.Repository<Customer>();
        var customer = await customers.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("El cliente no existe.");

        if (SalesRules.IsGenericCustomer(customer.DocumentType, customer.DocumentNumber))
        {
            throw new InvalidOperationException("El cliente CLIENTES VARIOS no se puede modificar.");
        }

        var data = CustomerReadModel.Normalize(
            request.DocumentType, request.DocumentNumber, request.FullName, request.Email, request.Phone, request.Address);

        if (await customers.AnyAsync(
                item => item.DocumentType == data.DocumentType && item.DocumentNumber == data.DocumentNumber && item.CustomerId != request.CustomerId,
                cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un cliente con ese documento.");
        }

        customer.DocumentType = data.DocumentType;
        customer.DocumentNumber = data.DocumentNumber;
        customer.FullName = data.FullName;
        customer.Email = data.Email;
        customer.Phone = data.Phone;
        customer.Address = data.Address;
        customer.IsActive = request.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return CustomerReadModel.ToDto(customer);
    }
}
