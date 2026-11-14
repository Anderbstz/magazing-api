using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class User
{
    public Guid UserId { get; set; }

    public Guid? BranchId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool EmailConfirmed { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<PurchaseOrder> PurchaseOrderApprovedByNavigations { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<PurchaseOrder> PurchaseOrderCreatedByNavigations { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Sale> SaleCancelledByNavigations { get; set; } = new List<Sale>();

    public virtual ICollection<Sale> SaleSellers { get; set; } = new List<Sale>();

    public virtual ICollection<StockAlert> StockAlerts { get; set; } = new List<StockAlert>();

    public virtual ICollection<Transfer> TransferCompletedByNavigations { get; set; } = new List<Transfer>();

    public virtual ICollection<Transfer> TransferRequestedByNavigations { get; set; } = new List<Transfer>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
