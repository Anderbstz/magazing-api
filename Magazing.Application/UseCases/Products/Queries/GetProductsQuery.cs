using System.Linq.Expressions;
using Magazing.Application.DTOs.Catalog;
using Magazing.Domain.Common;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Products.Queries;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    bool? IsActive = null) : IRequest<PagedResult<ProductDto>>;

internal sealed class GetProductsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToLower();
        var categoryId = request.CategoryId;
        var brandId = request.BrandId;
        var isActive = request.IsActive;

        Expression<Func<Product, bool>> predicate = item =>
            (search == null
                || item.Name.ToLower().Contains(search)
                || item.Sku.ToLower().Contains(search)
                || (item.Barcode != null && item.Barcode.Contains(search)))
            && (categoryId == null || item.CategoryId == categoryId)
            && (brandId == null || item.BrandId == brandId)
            && (isActive == null || item.IsActive == isActive);

        var page = await unitOfWork.Repository<Product>().GetPagedAsync(
            request.Page,
            request.PageSize,
            predicate,
            item => item.Name,
            cancellationToken: cancellationToken);

        var items = await ProductReadModel.ToDtosAsync(unitOfWork, page.Items, cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = items,
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }
}
