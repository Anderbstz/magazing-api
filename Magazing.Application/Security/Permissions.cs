namespace Magazing.Application.Security;

public static class Permissions
{
    public const string ClaimType = "permission";

    public const string UsersRead = "users.read";
    public const string UsersManage = "users.manage";

    public const string CatalogRead = "catalog.read";
    public const string CatalogManage = "catalog.manage";
    public const string PricesManage = "prices.manage";

    public const string InventoryRead = "inventory.read";
    public const string InventoryAdjust = "inventory.adjust";
    public const string TransfersManage = "transfers.manage";
    public const string WarehousesManage = "warehouses.manage";

    public const string SuppliersManage = "suppliers.manage";
    public const string PurchasesRead = "purchases.read";
    public const string PurchasesCreate = "purchases.create";
    public const string PurchasesApprove = "purchases.approve";
    public const string PurchasesReceive = "purchases.receive";

    public const string CustomersManage = "customers.manage";
    public const string SalesRead = "sales.read";
    public const string SalesCreate = "sales.create";
    public const string SalesCancel = "sales.cancel";

    public const string ReportsRead = "reports.read";
    public const string AlertsManage = "alerts.manage";
    public const string PredictionsRun = "predictions.run";
}
