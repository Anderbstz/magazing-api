namespace Magazing.Domain.Inventory;

public static class TransferStatuses
{
    public const string Pending = "PENDING";
    public const string Completed = "COMPLETED";
    public const string Cancelled = "CANCELLED";

    public static bool IsValid(string? status)
    {
        return status is Pending or Completed or Cancelled;
    }
}
