namespace Magazing.Domain.Inventory;

public static class MovementTypeCodes
{
    public const string Initial = "INITIAL";
    public const string PurchaseIn = "PURCHASE_IN";
    public const string SaleOut = "SALE_OUT";
    public const string TransferIn = "TRANSFER_IN";
    public const string TransferOut = "TRANSFER_OUT";
    public const string AdjustIn = "ADJUST_IN";
    public const string AdjustOut = "ADJUST_OUT";
    public const string SaleCancelIn = "SALE_CANCEL_IN";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Initial, PurchaseIn, SaleOut, TransferIn, TransferOut, AdjustIn, AdjustOut, SaleCancelIn
    };
}
