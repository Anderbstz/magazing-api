using System.Linq.Expressions;
using Magazing.Application.DTOs.Sales;
using Magazing.Domain.Common;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Customers.Queries;

public record GetCustomersQuery(int Page = 1, int PageSize = 20, string? Search = null, bool? IsActive = null) : IRequest<PagedResult<CustomerDto>>;

internal sealed class GetCustomersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToLower();
        var isActive = request.IsActive;

        Expression<Func<Customer, bool>> predicate = item =>
            (search == null || item.FullName.ToLower().Contains(search) || item.DocumentNumber.Contains(search))
            && (isActive == null || item.IsActive == isActive);

        var page = await unitOfWork.Repository<Customer>().GetPagedAsync(
            request.Page,
            request.PageSize,
            predicate,
            item => item.FullName,
            cancellationToken: cancellationToken);

        return page.Map(CustomerReadModel.ToDto);
    }
}
