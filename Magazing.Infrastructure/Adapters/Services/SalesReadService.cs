using Magazing.Application.DTOs.Sales;
using Magazing.Application.Services;
using Magazing.Domain.Common;
using Magazing.Domain.Entities;
using Magazing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Magazing.Infrastructure.Adapters.Services;

public class SalesReadService(MagazingDbContext context) : ISalesReadService
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<SaleSummaryDto>> GetSalesAsync(SaleFilter filter, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);

        var query = context.Sales.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(item => item.Status == filter.Status);
        }

        if (!string.IsNullOrWhiteSpace(filter.DocumentType))
        {
            query = query.Where(item => item.Invoice != null && item.Invoice.DocumentType == filter.DocumentType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(item => item.Invoice != null
                && (item.Invoice.FullNumber!.ToLower().Contains(term)
                    || item.Invoice.CustomerName.ToLower().Contains(term)
                    || item.Invoice.CustomerDocumentNumber.Contains(term)));
        }

        if (filter.FromUtc is { } from)
        {
            query = query.Where(item => item.SoldAt >= from);
        }

        if (filter.ToUtcExclusive is { } to)
        {
            query = query.Where(item => item.SoldAt < to);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await Project<SaleSummaryDto>(query
                .OrderByDescending(item => item.SoldAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<SaleSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<SaleDetailDto> GetSaleAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var sale = await Project<SaleDetailDto>(context.Sales.AsNoTracking().Where(item => item.SaleId == saleId))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("La venta no existe.");

        var extra = await context.Sales.AsNoTracking()
            .Where(item => item.SaleId == saleId)
            .Select(item => new
            {
                item.WarehouseId,
                WarehouseName = item.Warehouse.Name,
                item.Subtotal,
                item.DiscountTotal,
                item.TaxAmount,
                item.Notes,
                item.CancelledAt
            })
            .FirstAsync(cancellationToken);

        sale.WarehouseId = extra.WarehouseId;
        sale.WarehouseName = extra.WarehouseName;
        sale.Subtotal = extra.Subtotal;
        sale.DiscountTotal = extra.DiscountTotal;
        sale.TaxAmount = extra.TaxAmount;
        sale.Notes = extra.Notes;
        sale.CancelledAt = extra.CancelledAt;

        sale.Items = await context.SaleDetails.AsNoTracking()
            .Where(item => item.SaleId == saleId)
            .OrderBy(item => item.Product.Name)
            .Select(item => new SaleItemDto
            {
                ProductId = item.ProductId,
                Sku = item.Product.Sku,
                ProductName = item.Product.Name,
                UnitCode = item.Product.Unit.Code,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Discount = item.Discount,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                LineTotal = item.LineTotal
            })
            .ToListAsync(cancellationToken);

        sale.Payments = await context.Payments.AsNoTracking()
            .Where(item => item.SaleId == saleId)
            .OrderBy(item => item.PaidAt)
            .Select(item => new SalePaymentDto
            {
                PaymentMethodId = item.PaymentMethodId,
                MethodName = item.PaymentMethod.Name,
                Amount = item.Amount,
                Reference = item.Reference,
                PaidAt = item.PaidAt
            })
            .ToListAsync(cancellationToken);

        return sale;
    }

    private static IQueryable<T> Project<T>(IQueryable<Sale> query) where T : SaleSummaryDto, new()
    {
        return query.Select(item => new T
        {
            SaleId = item.SaleId,
            FullNumber = item.Invoice != null ? item.Invoice.FullNumber! : string.Empty,
            DocumentType = item.Invoice != null ? item.Invoice.DocumentType : string.Empty,
            CustomerName = item.Invoice != null ? item.Invoice.CustomerName : item.Customer.FullName,
            CustomerDocumentType = item.Invoice != null ? item.Invoice.CustomerDocumentType : item.Customer.DocumentType,
            CustomerDocumentNumber = item.Invoice != null ? item.Invoice.CustomerDocumentNumber : item.Customer.DocumentNumber,
            SellerName = item.Seller.FullName,
            Status = item.Status,
            ItemCount = item.SaleDetails.Count,
            Total = item.Total,
            PaymentMethods = item.Payments.Select(payment => payment.PaymentMethod.Name).Distinct().ToList(),
            SoldAt = item.SoldAt
        });
    }
}
