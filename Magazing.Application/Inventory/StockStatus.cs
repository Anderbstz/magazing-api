namespace Magazing.Application.Inventory;

public static class StockStatus
{
    public const string Ok = "OK";
    public const string Low = "LOW";
    public const string Out = "OUT";

    public static bool IsValid(string? status)
    {
        return status is Ok or Low or Out;
    }
}
