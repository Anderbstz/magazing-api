using Magazing.Application.DTOs.Purchases;
using Magazing.Domain.Common;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Unit = Magazing.Domain.Entities.Unit;

namespace Magazing.Application.UseCases.PurchaseOrders;

internal static class PurchaseOrderCalculator
{
    public static async Task<CalculatedOrder> CalculateAsync(
        IUnitOfWork unitOfWork,
        Guid supplierId,
        Guid warehouseId,
        DateOnly orderDate,
        DateOnly? expectedDate,
        List<PurchaseOrderItemRequest>? items,
        CancellationToken cancellationToken)
    {
        items ??= [];

        if (expectedDate is { } expected && expected < orderDate)
        {
            throw new InvalidOperationException("La fecha esperada no puede ser anterior a la fecha de la orden.");
        }

        if (!await unitOfWork.Repository<Supplier>().AnyAsync(item => item.SupplierId == supplierId && item.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("El proveedor no existe o esta inactivo.");
        }

        if (!await unitOfWork.Repository<Warehouse>().AnyAsync(item => item.WarehouseId == warehouseId && item.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("El almacen no existe o esta inactivo.");
        }

        if (items.Count == 0)
        {
            throw new InvalidOperationException("La orden debe tener al menos un producto.");
        }

        var productIds = items.Select(item => item.ProductId).ToList();
        if (productIds.Distinct().Count() != productIds.Count)
        {
            throw new InvalidOperationException("Un producto no puede repetirse en la misma orden.");
        }

        var products = (await unitOfWork.Repository<Product>().FindAsync(
                item => productIds.Contains(item.ProductId) && item.IsActive,
                cancellationToken))
            .ToDictionary(item => item.ProductId);

        if (products.Count != productIds.Count)
        {
            throw new InvalidOperationException("Uno o mas productos no existen o estan inactivos.");
        }

        var unitIds = products.Values.Select(item => item.UnitId).Distinct().ToList();
        var decimalUnits = (await unitOfWork.Repository<Unit>().FindAsync(item => unitIds.Contains(item.UnitId), cancellationToken))
            .ToDictionary(item => item.UnitId, item => item.AllowsDecimals);

        var lines = new List<CalculatedLine>();
        decimal taxableSubtotal = 0;

        foreach (var item in items)
        {
            var product = products[item.ProductId];

            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException($"La cantidad de {product.Sku} debe ser mayor a cero.");
            }

            if (!decimalUnits.GetValueOrDefault(product.UnitId) && item.Quantity != decimal.Truncate(item.Quantity))
            {
                throw new InvalidOperationException($"La unidad de {product.Sku} no admite cantidades con decimales.");
            }

            if (item.UnitCost < 0 || decimal.Round(item.UnitCost, 4) != item.UnitCost)
            {
                throw new InvalidOperationException($"El costo de {product.Sku} debe ser positivo y tener como maximo 4 decimales.");
            }

            var subtotal = decimal.Round(item.Quantity * item.UnitCost, 2);
            lines.Add(new CalculatedLine(item.ProductId, item.Quantity, item.UnitCost, subtotal));

            if (product.IsTaxable)
            {
                taxableSubtotal += subtotal;
            }
        }

        var orderSubtotal = lines.Sum(line => line.Subtotal);
        var taxAmount = decimal.Round(taxableSubtotal * TaxRates.Igv, 2);

        return new CalculatedOrder(lines, orderSubtotal, taxAmount, orderSubtotal + taxAmount);
    }

    public static async Task AddDetailsAsync(IUnitOfWork unitOfWork, Guid purchaseOrderId, IEnumerable<CalculatedLine> lines, CancellationToken cancellationToken)
    {
        foreach (var line in lines)
        {
            await unitOfWork.Repository<PurchaseOrderDetail>().AddAsync(new PurchaseOrderDetail
            {
                PurchaseOrderDetailId = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrderId,
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                Subtotal = line.Subtotal
            }, cancellationToken);
        }
    }

    public sealed record CalculatedLine(Guid ProductId, decimal Quantity, decimal UnitCost, decimal Subtotal);

    public sealed record CalculatedOrder(List<CalculatedLine> Lines, decimal Subtotal, decimal TaxAmount, decimal Total);
}
