using Magazing.Application.DTOs.Inventory;
using Magazing.Application.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Common;
using Magazing.Domain.Entities;
using Magazing.Domain.Inventory;
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

    public async Task<PagedResult<MovementDto>> GetMovementsAsync(MovementFilter filter, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);

        var query = context.InventoryMovements.AsNoTracking();

        if (filter.ProductId is { } productId)
        {
            query = query.Where(item => item.ProductId == productId);
        }

        if (filter.WarehouseId is { } warehouseId)
        {
            query = query.Where(item => item.WarehouseId == warehouseId);
        }

        if (!string.IsNullOrWhiteSpace(filter.MovementTypeCode))
        {
            query = query.Where(item => item.MovementType.Code == filter.MovementTypeCode);
        }

        if (filter.FromUtc is { } from)
        {
            query = query.Where(item => item.CreatedAt >= from);
        }

        if (filter.ToUtcExclusive is { } to)
        {
            query = query.Where(item => item.CreatedAt < to);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await ProjectMovements(query
                .OrderByDescending(item => item.CreatedAt)
                .ThenByDescending(item => item.InventoryMovementId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<MovementDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MovementDto> GetMovementAsync(Guid inventoryMovementId, CancellationToken cancellationToken = default)
    {
        return await ProjectMovements(context.InventoryMovements.AsNoTracking().Where(item => item.InventoryMovementId == inventoryMovementId))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("El movimiento no existe.");
    }

    public async Task<PagedResult<TransferSummaryDto>> GetTransfersAsync(string? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = context.Transfers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(item => item.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await ProjectTransfers<TransferSummaryDto>(query
                .OrderByDescending(item => item.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<TransferSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TransferDetailDto> GetTransferAsync(Guid transferId, CancellationToken cancellationToken = default)
    {
        var transfer = await ProjectTransfers<TransferDetailDto>(context.Transfers.AsNoTracking().Where(item => item.TransferId == transferId))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("La transferencia no existe.");

        var originWarehouseId = transfer.OriginWarehouseId;

        transfer.Items = await context.TransferDetails.AsNoTracking()
            .Where(item => item.TransferId == transferId)
            .OrderBy(item => item.Product.Name)
            .Select(item => new TransferItemDto
            {
                ProductId = item.ProductId,
                Sku = item.Product.Sku,
                ProductName = item.Product.Name,
                UnitCode = item.Product.Unit.Code,
                Quantity = item.Quantity,
                AvailableAtOrigin = context.Stocks
                    .Where(stock => stock.ProductId == item.ProductId && stock.WarehouseId == originWarehouseId)
                    .Select(stock => stock.Quantity)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return transfer;
    }

    private static IQueryable<T> ProjectTransfers<T>(IQueryable<Transfer> query) where T : TransferSummaryDto, new()
    {
        return query.Select(item => new T
        {
            TransferId = item.TransferId,
            Code = item.Code,
            OriginWarehouseId = item.OriginWarehouseId,
            OriginWarehouseName = item.OriginWarehouse.Name,
            DestinationWarehouseId = item.DestinationWarehouseId,
            DestinationWarehouseName = item.DestinationWarehouse.Name,
            Status = item.Status,
            ItemCount = item.TransferDetails.Count,
            TotalQuantity = item.TransferDetails.Sum(detail => detail.Quantity),
            Notes = item.Notes,
            RequestedByName = item.RequestedByNavigation != null ? item.RequestedByNavigation.FullName : "Sistema",
            CompletedByName = item.CompletedByNavigation != null ? item.CompletedByNavigation.FullName : null,
            CreatedAt = item.CreatedAt,
            CompletedAt = item.CompletedAt
        });
    }

    private IQueryable<MovementDto> ProjectMovements(IQueryable<InventoryMovement> query)
    {
        return query.Select(item => new MovementDto
        {
            InventoryMovementId = item.InventoryMovementId,
            ProductId = item.ProductId,
            ProductName = item.Product.Name,
            Sku = item.Product.Sku,
            WarehouseId = item.WarehouseId,
            WarehouseCode = item.Warehouse.Code,
            MovementTypeCode = item.MovementType.Code,
            MovementTypeName = item.MovementType.Name,
            Direction = item.MovementType.Direction,
            Quantity = item.Quantity,
            UnitCost = item.UnitCost,
            BalanceAfter = item.BalanceAfter,
            AverageCostAfter = item.AverageCostAfter,
            ReferenceType = item.ReferenceType,
            ReferenceId = item.ReferenceId,
            ReferenceCode = item.ReferenceType == ReferenceTypes.Transfer
                ? context.Transfers.Where(transfer => transfer.TransferId == item.ReferenceId).Select(transfer => transfer.Code).FirstOrDefault()
                : item.ReferenceType == ReferenceTypes.PurchaseOrder
                    ? context.PurchaseOrders.Where(order => order.PurchaseOrderId == item.ReferenceId).Select(order => order.Code).FirstOrDefault()
                    : item.ReferenceType == ReferenceTypes.Sale
                        ? context.Invoices.Where(invoice => invoice.SaleId == item.ReferenceId).Select(invoice => invoice.FullNumber).FirstOrDefault()
                        : null,
            Notes = item.Notes,
            CreatedByName = item.CreatedByNavigation != null ? item.CreatedByNavigation.FullName : "Sistema",
            CreatedAt = item.CreatedAt
        });
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
