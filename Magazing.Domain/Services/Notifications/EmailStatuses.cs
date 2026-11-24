namespace Magazing.Domain.Services.Notifications;

public static class EmailStatuses
{
    public const string Pending = "PENDING";
    public const string Sent = "SENT";
    public const string Failed = "FAILED";

    public static bool IsValid(string? status)
    {
        return status is Pending or Sent or Failed;
    }
}

public static class EmailReferenceTypes
{
    public const string Sale = "SALE";
}

public static class EmailTemplateCodes
{
    public const string Welcome = "WELCOME";
    public const string AccessReset = "ACCESS_RESET";
    public const string Invoice = "INVOICE";
    public const string StockAlert = "STOCK_ALERT";
}
