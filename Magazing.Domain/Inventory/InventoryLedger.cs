using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;

namespace Magazing.Domain.Inventory;

public class InventoryLedger(IUnitOfWork unitOfWork)
{
    private const int CostDecimals = 4;

    private readonly Dictionary<string, MovementType> _movementTypes = new();
    private readonly Dictionary<Guid, Warehouse> _warehouses = new();
    private readonly Dictionary<Guid, ProductState> _products = new();
    private readonly Dictionary<(Guid ProductId, Guid WarehouseId), Stock> _stocks = new();

    public async Task<InventoryMovement> RegisterAsync(InventoryMovementRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("La cantidad debe ser mayor a cero.");
        }

        var movementType = await GetMovementTypeAsync(request.MovementTypeCode, cancellationToken);
        var warehouse = await GetWarehouseAsync(request.WarehouseId, cancellationToken);
        var product = await GetProductAsync(request.ProductId, cancellationToken);

        if (!product.AllowsDecimals && request.Quantity != decimal.Truncate(request.Quantity))
        {
            throw new InvalidOperationException($"La unidad de {product.Entity.Sku} no admite cantidades con decimales.");
        }

        var stock = await GetStockAsync(request.ProductId, request.WarehouseId, cancellationToken);
        decimal unitCost;

        if (movementType.Direction < 0)
        {
            if (stock.Quantity < request.Quantity)
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente de {product.Entity.Sku} en {warehouse.Code}: disponible {stock.Quantity:0.###}, solicitado {request.Quantity:0.###}.");
            }

            unitCost = product.Entity.AverageCost;
            stock.Quantity -= request.Quantity;
            product.TotalQuantity -= request.Quantity;
        }
        else
        {
            unitCost = request.UnitCost ?? product.Entity.AverageCost;
            if (unitCost < 0)
            {
                throw new InvalidOperationException("El costo unitario no puede ser negativo.");
            }

            product.Entity.AverageCost = CalculateAverageCost(product.TotalQuantity, product.Entity.AverageCost, request.Quantity, unitCost);
            stock.Quantity += request.Quantity;
            product.TotalQuantity += request.Quantity;
        }

        var now = DateTime.UtcNow;
        stock.UpdatedAt = now;

        var movement = new InventoryMovement
        {
            InventoryMovementId = Guid.NewGuid(),
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            MovementTypeId = movementType.MovementTypeId,
            Quantity = request.Quantity,
            UnitCost = decimal.Round(unitCost, CostDecimals),
            BalanceAfter = stock.Quantity,
            AverageCostAfter = product.Entity.AverageCost,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            Notes = request.Notes,
            CreatedBy = request.UserId,
            CreatedAt = now
        };

        await unitOfWork.Repository<InventoryMovement>().AddAsync(movement, cancellationToken);

        return movement;
    }

    public static decimal CalculateAverageCost(decimal currentQuantity, decimal currentAverageCost, decimal incomingQuantity, decimal incomingUnitCost)
    {
        if (currentQuantity <= 0)
        {
            return decimal.Round(incomingUnitCost, CostDecimals);
        }

        var totalCost = currentQuantity * currentAverageCost + incomingQuantity * incomingUnitCost;
        return decimal.Round(totalCost / (currentQuantity + incomingQuantity), CostDecimals);
    }

    private async Task<MovementType> GetMovementTypeAsync(string code, CancellationToken cancellationToken)
    {
        if (_movementTypes.TryGetValue(code, out var cached))
        {
            return cached;
        }

        var movementType = await unitOfWork.Repository<MovementType>().FirstOrDefaultAsync(item => item.Code == code, cancellationToken)
            ?? throw new InvalidOperationException($"El tipo de movimiento {code} no existe.");

        _movementTypes[code] = movementType;
        return movementType;
    }

    private async Task<Warehouse> GetWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken)
    {
        if (_warehouses.TryGetValue(warehouseId, out var cached))
        {
            return cached;
        }

        var warehouse = await unitOfWork.Repository<Warehouse>().FirstOrDefaultAsync(item => item.WarehouseId == warehouseId, cancellationToken)
            ?? throw new KeyNotFoundException("El almacen no existe.");

        if (!warehouse.IsActive)
        {
            throw new InvalidOperationException($"El almacen {warehouse.Code} esta inactivo.");
        }

        _warehouses[warehouseId] = warehouse;
        return warehouse;
    }

    private async Task<ProductState> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        if (_products.TryGetValue(productId, out var cached))
        {
            return cached;
        }

        var products = unitOfWork.Repository<Product>();
        var product = await products.FirstOrDefaultAsync(item => item.ProductId == productId, cancellationToken)
            ?? throw new KeyNotFoundException("El producto no existe.");

        var unit = await unitOfWork.Repository<Unit>().FirstOrDefaultAsync(item => item.UnitId == product.UnitId, cancellationToken);
        var totalQuantity = (await unitOfWork.Repository<Stock>().FindAsync(item => item.ProductId == productId, cancellationToken))
            .Sum(item => item.Quantity);

        products.Update(product);

        var state = new ProductState(product, unit?.AllowsDecimals ?? false) { TotalQuantity = totalQuantity };
        _products[productId] = state;
        return state;
    }

    private async Task<Stock> GetStockAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken)
    {
        if (_stocks.TryGetValue((productId, warehouseId), out var cached))
        {
            return cached;
        }

        var stocks = unitOfWork.Repository<Stock>();
        var stock = await stocks.FirstOrDefaultAsync(item => item.ProductId == productId && item.WarehouseId == warehouseId, cancellationToken);

        if (stock is null)
        {
            stock = new Stock
            {
                StockId = Guid.NewGuid(),
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = 0,
                UpdatedAt = DateTime.UtcNow
            };

            await stocks.AddAsync(stock, cancellationToken);
        }
        else
        {
            stocks.Update(stock);
        }

        _stocks[(productId, warehouseId)] = stock;
        return stock;
    }

    private sealed class ProductState(Product entity, bool allowsDecimals)
    {
        public Product Entity { get; } = entity;

        public bool AllowsDecimals { get; } = allowsDecimals;

        public decimal TotalQuantity { get; set; }
    }
}
