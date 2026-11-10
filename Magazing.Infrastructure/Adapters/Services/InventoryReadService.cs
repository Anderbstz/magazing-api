using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Common;
using Magazing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Magazing.Infrastructure.Adapters.Services;

public class InventoryReadService(MagazingDbContext context) : IInventoryReadService
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<StockItemDto>> GetStocksAsync(
        Guid warehouseId,
        string? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = StockRows(warehouseId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(row => row.ProductName.ToLower().Contains(term) || row.Sku.ToLower().Contains(term));
        }

        query = status switch
        {
            StockStatus.Out => query.Where(row => row.Quantity <= 0),
            StockStatus.Low => query.Where(row => row.Quantity > 0 && row.Quantity <= row.MinStock),
            StockStatus.Ok => query.Where(row => row.Quantity > row.MinStock),
            _ => query
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(row => row.ProductName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new StockItemDto
            {
                ProductId = row.ProductId,
                Sku = row.Sku,
                ProductName = row.ProductName,
                UnitCode = row.UnitCode,
                CategoryName = row.CategoryName,
                WarehouseId = warehouseId,
                Quantity = row.Quantity,
                MinStock = row.MinStock,
                MaxStock = row.MaxStock,
                AverageCost = row.AverageCost,
                StockValue = Math.Round(row.Quantity * row.AverageCost, 2),
                Status = row.Quantity <= 0 ? StockStatus.Out : row.Quantity <= row.MinStock ? StockStatus.Low : StockStatus.Ok
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<StockItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<StockSummaryDto> GetStockSummaryAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var rows = await StockRows(warehouseId)
            .Select(row => new { row.Quantity, row.MinStock, row.AverageCost })
            .ToListAsync(cancellationToken);

        return new StockSummaryDto
        {
            ProductCount = rows.Count,
            TotalUnits = rows.Sum(row => row.Quantity),
            TotalValue = Math.Round(rows.Sum(row => row.Quantity * row.AverageCost), 2),
            LowCount = rows.Count(row => row.Quantity > 0 && row.Quantity <= row.MinStock),
            OutCount = rows.Count(row => row.Quantity <= 0)
        };
    }

    private IQueryable<StockRow> StockRows(Guid warehouseId)
    {
        return from product in context.Products.AsNoTracking()
            join stock in context.Stocks.AsNoTracking().Where(item => item.WarehouseId == warehouseId)
                on product.ProductId equals stock.ProductId into productStocks
            from stock in productStocks.DefaultIfEmpty()
            let quantity = stock == null ? 0m : stock.Quantity
            where product.IsActive || quantity > 0
            select new StockRow
            {
                ProductId = product.ProductId,
                Sku = product.Sku,
                ProductName = product.Name,
                UnitCode = product.Unit.Code,
                CategoryName = product.Category.Name,
                Quantity = quantity,
                MinStock = product.MinStock,
                MaxStock = product.MaxStock,
                AverageCost = product.AverageCost
            };
    }

    private sealed class StockRow
    {
        public Guid ProductId { get; init; }

        public string Sku { get; init; } = null!;

        public string ProductName { get; init; } = null!;

        public string UnitCode { get; init; } = null!;

        public string CategoryName { get; init; } = null!;

        public decimal Quantity { get; init; }

        public decimal MinStock { get; init; }

        public decimal? MaxStock { get; init; }

        public decimal AverageCost { get; init; }
    }
}
