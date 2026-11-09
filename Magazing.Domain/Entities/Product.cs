using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class Product
{
    public Guid ProductId { get; set; }

    public Guid CategoryId { get; set; }

    public Guid? BrandId { get; set; }

    public Guid UnitId { get; set; }

    public string Sku { get; set; } = null!;

    public string? Barcode { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsTaxable { get; set; }

    public decimal MinStock { get; set; }

    public decimal? MaxStock { get; set; }

    public decimal AverageCost { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<DemandPrediction> DemandPredictions { get; set; } = new List<DemandPrediction>();

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    public virtual ICollection<StockAlert> StockAlerts { get; set; } = new List<StockAlert>();

    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();

    public virtual ICollection<TransferDetail> TransferDetails { get; set; } = new List<TransferDetail>();

    public virtual Unit Unit { get; set; } = null!;
}
