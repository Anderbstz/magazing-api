namespace Magazing.Domain.Sales;

public static class SalesRules
{
    public const string GenericCustomerDocumentType = "DNI";
    public const string GenericCustomerDocumentNumber = "00000000";
    public const decimal BoletaIdentificationThreshold = 700m;
    public const string Currency = "PEN";
    public const string CashPaymentCode = "CASH";

    public static bool IsGenericCustomer(string documentType, string documentNumber)
    {
        return documentType == GenericCustomerDocumentType && documentNumber == GenericCustomerDocumentNumber;
    }
}

public static class DocumentTypes
{
    public const string Dni = "DNI";
    public const string Ruc = "RUC";
    public const string ForeignerCard = "CE";
    public const string Passport = "PASSPORT";

    public static bool IsValid(string? type)
    {
        return type is Dni or Ruc or ForeignerCard or Passport;
    }
}

public static class InvoiceTypes
{
    public const string Boleta = "BOLETA";
    public const string Factura = "FACTURA";

    public static bool IsValid(string? type)
    {
        return type is Boleta or Factura;
    }
}

public static class SaleStatuses
{
    public const string Completed = "COMPLETED";
    public const string Cancelled = "CANCELLED";

    public static bool IsValid(string? status)
    {
        return status is Completed or Cancelled;
    }
}

public static class InvoiceStatuses
{
    public const string Issued = "ISSUED";
    public const string Cancelled = "CANCELLED";
}
