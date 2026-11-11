namespace Magazing.Domain.Purchases;

public static class PurchaseOrderStatuses
{
    public const string Draft = "DRAFT";
    public const string Approved = "APPROVED";
    public const string Received = "RECEIVED";
    public const string Cancelled = "CANCELLED";

    public static bool IsValid(string? status)
    {
        return status is Draft or Approved or Received or Cancelled;
    }
}
