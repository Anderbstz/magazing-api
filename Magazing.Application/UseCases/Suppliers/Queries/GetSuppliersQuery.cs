using System.Linq.Expressions;
using Magazing.Application.DTOs.Purchases;
using Magazing.Domain.Common;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Suppliers.Queries;

public record GetSuppliersQuery(int Page = 1, int PageSize = 20, string? Search = null, bool? IsActive = null) : IRequest<PagedResult<SupplierDto>>;

internal sealed class GetSuppliersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSuppliersQuery, PagedResult<SupplierDto>>
{
    public async Task<PagedResult<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToLower();
        var isActive = request.IsActive;

        Expression<Func<Supplier, bool>> predicate = item =>
            (search == null || item.BusinessName.ToLower().Contains(search) || item.Ruc.Contains(search))
            && (isActive == null || item.IsActive == isActive);

        var page = await unitOfWork.Repository<Supplier>().GetPagedAsync(
            request.Page,
            request.PageSize,
            predicate,
            item => item.BusinessName,
            cancellationToken: cancellationToken);

        return page.Map(SupplierReadModel.ToDto);
    }
}
