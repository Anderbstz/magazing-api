using Magazing.Application.DTOs.Purchases;
using Magazing.Application.Services;
using Magazing.Domain.Common;
using Magazing.Domain.Entities;
using Magazing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Magazing.Infrastructure.Adapters.Services;

public class PurchaseReadService(MagazingDbContext context) : IPurchaseReadService
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<PurchaseOrderSummaryDto>> GetOrdersAsync(
        string? status,
        Guid? supplierId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = context.PurchaseOrders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(item => item.Status == status);
        }

        if (supplierId is { } id)
        {
            query = query.Where(item => item.SupplierId == id);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item => item.Code.ToLower().Contains(term) || item.Supplier.BusinessName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await Project<PurchaseOrderSummaryDto>(query
                .OrderByDescending(item => item.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<PurchaseOrderSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PurchaseOrderDetailDto> GetOrderAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var order = await Project<PurchaseOrderDetailDto>(context.PurchaseOrders.AsNoTracking().Where(item => item.PurchaseOrderId == purchaseOrderId))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("La orden de compra no existe.");

        var extra = await context.PurchaseOrders.AsNoTracking()
            .Where(item => item.PurchaseOrderId == purchaseOrderId)
            .Select(item => new
            {
                item.Supplier.Ruc,
                item.Notes,
                ApprovedByName = item.ApprovedByNavigation != null ? item.ApprovedByNavigation.FullName : null,
                item.ApprovedAt,
                item.ReceivedAt
            })
            .FirstAsync(cancellationToken);

        order.SupplierRuc = extra.Ruc;
        order.Notes = extra.Notes;
        order.ApprovedByName = extra.ApprovedByName;
        order.ApprovedAt = extra.ApprovedAt;
        order.ReceivedAt = extra.ReceivedAt;

        order.Items = await context.PurchaseOrderDetails.AsNoTracking()
            .Where(item => item.PurchaseOrderId == purchaseOrderId)
            .OrderBy(item => item.Product.Name)
            .Select(item => new PurchaseOrderItemDto
            {
                ProductId = item.ProductId,
                Sku = item.Product.Sku,
                ProductName = item.Product.Name,
                UnitCode = item.Product.Unit.Code,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                Subtotal = item.Subtotal
            })
            .ToListAsync(cancellationToken);

        return order;
    }

    private static IQueryable<T> Project<T>(IQueryable<PurchaseOrder> query) where T : PurchaseOrderSummaryDto, new()
    {
        return query.Select(item => new T
        {
            PurchaseOrderId = item.PurchaseOrderId,
            Code = item.Code,
            SupplierId = item.SupplierId,
            SupplierName = item.Supplier.BusinessName,
            WarehouseId = item.WarehouseId,
            WarehouseName = item.Warehouse.Name,
            Status = item.Status,
            OrderDate = item.OrderDate,
            ExpectedDate = item.ExpectedDate,
            Subtotal = item.Subtotal,
            TaxAmount = item.TaxAmount,
            Total = item.Total,
            ItemCount = item.PurchaseOrderDetails.Count,
            CreatedByName = item.CreatedByNavigation != null ? item.CreatedByNavigation.FullName : "Sistema",
            CreatedAt = item.CreatedAt
        });
    }
}
