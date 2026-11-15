using Magazing.Domain.Common;

namespace Magazing.Domain.Sales;

public static class SalePricing
{
    public static SaleLineAmounts CalculateLine(decimal quantity, decimal unitPrice, decimal discount, bool isTaxable)
    {
        if (discount < 0 || decimal.Round(discount, 2) != discount)
        {
            throw new InvalidOperationException("El descuento debe ser positivo y tener como maximo 2 decimales.");
        }

        var gross = decimal.Round(quantity * unitPrice, 2);
        if (discount > gross)
        {
            throw new InvalidOperationException("El descuento no puede ser mayor que el importe de la linea.");
        }

        var lineTotal = gross - discount;
        var taxRate = isTaxable ? TaxRates.Igv : 0m;
        var taxableBase = isTaxable ? decimal.Round(lineTotal / (1 + taxRate), 2) : lineTotal;

        return new SaleLineAmounts(taxableBase, discount, taxRate, lineTotal - taxableBase, lineTotal);
    }

    public static decimal CalculateChange(decimal total, decimal paid)
    {
        if (paid < total)
        {
            throw new InvalidOperationException($"El pago es insuficiente: total {total:0.00}, pagado {paid:0.00}.");
        }

        return paid - total;
    }
}

public sealed record SaleLineAmounts(decimal TaxableBase, decimal Discount, decimal TaxRate, decimal TaxAmount, decimal LineTotal);
