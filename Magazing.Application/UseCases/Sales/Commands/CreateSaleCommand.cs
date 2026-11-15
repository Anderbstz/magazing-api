using Magazing.Application.DTOs.Sales;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Inventory;
using Magazing.Domain.Repositories;
using Magazing.Domain.Sales;
using MediatR;

namespace Magazing.Application.UseCases.Sales.Commands;

public record CreateSaleCommand(
    Guid WarehouseId,
    Guid CustomerId,
    string DocumentType,
    string? Notes,
    List<SaleItemRequest> Items,
    List<SalePaymentRequest> Payments,
    Guid SellerId) : IRequest<SaleDetailDto>;

internal sealed class CreateSaleCommandHandler(
    IUnitOfWork unitOfWork,
    InventoryLedger inventoryLedger,
    ISalesReadService salesReadService)
    : IRequestHandler<CreateSaleCommand, SaleDetailDto>
{
    public async Task<SaleDetailDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var documentType = request.DocumentType?.Trim().ToUpper();
        var items = request.Items ?? [];
        var payments = request.Payments ?? [];

        if (!InvoiceTypes.IsValid(documentType))
        {
            throw new InvalidOperationException("El comprobante debe ser BOLETA o FACTURA.");
        }

        if (items.Count == 0)
        {
            throw new InvalidOperationException("La venta debe tener al menos un producto.");
        }

        var productIds = items.Select(item => item.ProductId).ToList();
        if (productIds.Distinct().Count() != productIds.Count)
        {
            throw new InvalidOperationException("Un producto no puede repetirse en la misma venta.");
        }

        if (payments.Count == 0)
        {
            throw new InvalidOperationException("La venta debe tener al menos un pago.");
        }

        var warehouse = await unitOfWork.Repository<Warehouse>().FirstOrDefaultAsync(
                item => item.WarehouseId == request.WarehouseId && item.IsActive,
                cancellationToken)
            ?? throw new InvalidOperationException("El almacen no existe o esta inactivo.");

        var customer = await unitOfWork.Repository<Customer>().FirstOrDefaultAsync(
                item => item.CustomerId == request.CustomerId && item.IsActive,
                cancellationToken)
            ?? throw new InvalidOperationException("El cliente no existe o esta inactivo.");

        if (documentType == InvoiceTypes.Factura && customer.DocumentType != DocumentTypes.Ruc)
        {
            throw new InvalidOperationException("La FACTURA solo se emite a clientes con RUC.");
        }

        var products = (await unitOfWork.Repository<Product>().FindAsync(
                item => productIds.Contains(item.ProductId) && item.IsActive,
                cancellationToken))
            .ToDictionary(item => item.ProductId);

        if (products.Count != productIds.Count)
        {
            throw new InvalidOperationException("Uno o mas productos no existen o estan inactivos.");
        }

        var prices = (await unitOfWork.Repository<ProductPrice>().FindAsync(
                item => productIds.Contains(item.ProductId) && item.ValidTo == null,
                cancellationToken))
            .GroupBy(item => item.ProductId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.ValidFrom).First().Price);

        var saleId = Guid.NewGuid();
        var lines = new List<(SaleItemRequest Item, decimal UnitPrice, SaleLineAmounts Amounts)>();

        foreach (var item in items)
        {
            var product = products[item.ProductId];
            if (!prices.TryGetValue(item.ProductId, out var unitPrice))
            {
                throw new InvalidOperationException($"El producto {product.Sku} no tiene un precio vigente.");
            }

            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException($"La cantidad de {product.Sku} debe ser mayor a cero.");
            }

            lines.Add((item, unitPrice, SalePricing.CalculateLine(item.Quantity, unitPrice, item.Discount, product.IsTaxable)));
        }

        var total = lines.Sum(line => line.Amounts.LineTotal);

        if (documentType == InvoiceTypes.Boleta
            && total > SalesRules.BoletaIdentificationThreshold
            && SalesRules.IsGenericCustomer(customer.DocumentType, customer.DocumentNumber))
        {
            throw new InvalidOperationException(
                $"Una boleta mayor a S/ {SalesRules.BoletaIdentificationThreshold:0.00} requiere identificar al cliente.");
        }

        var paymentRows = await BuildPaymentsAsync(payments, total, cancellationToken);
        var change = SalePricing.CalculateChange(total, payments.Sum(payment => payment.Amount));

        var series = (await unitOfWork.Repository<DocumentSeries>().FindAsync(
                item => item.BranchId == warehouse.BranchId && item.DocumentType == documentType && item.IsActive,
                cancellationToken))
            .OrderBy(item => item.Series)
            .FirstOrDefault()
            ?? throw new InvalidOperationException($"La sucursal no tiene una serie activa de {documentType}.");

        series.CurrentNumber += 1;
        unitOfWork.Repository<DocumentSeries>().Update(series);

        var now = DateTime.UtcNow;
        var sale = new Sale
        {
            SaleId = saleId,
            BranchId = warehouse.BranchId,
            WarehouseId = warehouse.WarehouseId,
            CustomerId = customer.CustomerId,
            SellerId = request.SellerId,
            Status = SaleStatuses.Completed,
            Subtotal = lines.Sum(line => line.Amounts.TaxableBase),
            DiscountTotal = lines.Sum(line => line.Amounts.Discount),
            TaxAmount = lines.Sum(line => line.Amounts.TaxAmount),
            Total = total,
            Notes = request.Notes?.Trim(),
            SoldAt = now,
            CreatedAt = now
        };

        await unitOfWork.Repository<Sale>().AddAsync(sale, cancellationToken);

        foreach (var (item, unitPrice, amounts) in lines)
        {
            var movement = await inventoryLedger.RegisterAsync(new InventoryMovementRequest(
                item.ProductId,
                warehouse.WarehouseId,
                MovementTypeCodes.SaleOut,
                item.Quantity,
                null,
                ReferenceTypes.Sale,
                saleId,
                null,
                request.SellerId), cancellationToken);

            await unitOfWork.Repository<SaleDetail>().AddAsync(new SaleDetail
            {
                SaleDetailId = Guid.NewGuid(),
                SaleId = saleId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                Discount = amounts.Discount,
                TaxRate = amounts.TaxRate,
                TaxAmount = amounts.TaxAmount,
                UnitCost = movement.UnitCost,
                LineTotal = amounts.LineTotal
            }, cancellationToken);
        }

        await unitOfWork.Repository<Invoice>().AddAsync(new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            SaleId = saleId,
            DocumentSeriesId = series.DocumentSeriesId,
            DocumentType = documentType!,
            Series = series.Series,
            Number = series.CurrentNumber,
            CustomerDocumentType = customer.DocumentType,
            CustomerDocumentNumber = customer.DocumentNumber,
            CustomerName = customer.FullName,
            Currency = SalesRules.Currency,
            Subtotal = sale.Subtotal,
            TaxAmount = sale.TaxAmount,
            Total = sale.Total,
            Status = InvoiceStatuses.Issued,
            IssuedAt = now
        }, cancellationToken);

        foreach (var payment in paymentRows)
        {
            payment.SaleId = saleId;
            payment.PaidAt = now;
            await unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        var result = await salesReadService.GetSaleAsync(saleId, cancellationToken);
        result.Change = change;
        return result;
    }

    private async Task<List<Payment>> BuildPaymentsAsync(List<SalePaymentRequest> payments, decimal total, CancellationToken cancellationToken)
    {
        if (payments.Any(payment => payment.Amount <= 0 || decimal.Round(payment.Amount, 2) != payment.Amount))
        {
            throw new InvalidOperationException("Cada pago debe ser mayor a cero y tener como maximo 2 decimales.");
        }

        var methodIds = payments.Select(payment => payment.PaymentMethodId).Distinct().ToList();
        var methods = (await unitOfWork.Repository<PaymentMethod>().FindAsync(
                item => methodIds.Contains(item.PaymentMethodId) && item.IsActive,
                cancellationToken))
            .ToDictionary(item => item.PaymentMethodId);

        if (methods.Count != methodIds.Count)
        {
            throw new InvalidOperationException("Uno o mas metodos de pago no existen o estan inactivos.");
        }

        var change = SalePricing.CalculateChange(total, payments.Sum(payment => payment.Amount));
        var cashPaid = payments
            .Where(payment => methods[payment.PaymentMethodId].Code == SalesRules.CashPaymentCode)
            .Sum(payment => payment.Amount);

        if (change > cashPaid)
        {
            throw new InvalidOperationException("Solo se puede pagar de mas en efectivo; el resto de metodos debe cubrir el monto exacto.");
        }

        var rows = new List<Payment>();
        var pendingChange = change;

        foreach (var payment in payments.AsEnumerable().Reverse())
        {
            var amount = payment.Amount;
            if (pendingChange > 0 && methods[payment.PaymentMethodId].Code == SalesRules.CashPaymentCode)
            {
                var applied = Math.Min(amount, pendingChange);
                amount -= applied;
                pendingChange -= applied;
            }

            if (amount > 0)
            {
                rows.Insert(0, new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    PaymentMethodId = payment.PaymentMethodId,
                    Amount = amount,
                    Reference = string.IsNullOrWhiteSpace(payment.Reference) ? null : payment.Reference.Trim()
                });
            }
        }

        return rows;
    }
}
