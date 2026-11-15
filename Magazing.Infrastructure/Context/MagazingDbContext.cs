using System;
using System.Collections.Generic;
using Magazing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Magazing.Infrastructure.Context;

public partial class MagazingDbContext : DbContext
{
    public MagazingDbContext(DbContextOptions<MagazingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<DemandPrediction> DemandPredictions { get; set; }

    public virtual DbSet<DocumentSeries> DocumentSeries { get; set; }

    public virtual DbSet<EmailOutbox> EmailOutboxes { get; set; }

    public virtual DbSet<InventoryMovement> InventoryMovements { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<MovementType> MovementTypes { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductPrice> ProductPrices { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleDetail> SaleDetails { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<StockAlert> StockAlerts { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Transfer> Transfers { get; set; }

    public virtual DbSet<TransferDetail> TransferDetails { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.BranchId).HasName("branches_pkey");

            entity.ToTable("branches");

            entity.HasIndex(e => e.Code, "branches_code_key").IsUnique();

            entity.HasIndex(e => e.CompanyId, "idx_branches_company_id");

            entity.Property(e => e.BranchId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("branch_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Company).WithMany(p => p.Branches)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("branches_company_id_fkey");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("brands_pkey");

            entity.ToTable("brands");

            entity.HasIndex(e => e.Name, "brands_name_key").IsUnique();

            entity.Property(e => e.BrandId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("brand_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.HasIndex(e => e.Name, "categories_name_key").IsUnique();

            entity.HasIndex(e => e.ParentCategoryId, "idx_categories_parent_category_id");

            entity.Property(e => e.CategoryId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("categories_parent_category_id_fkey");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("companies_pkey");

            entity.ToTable("companies");

            entity.HasIndex(e => e.Ruc, "companies_ruc_key").IsUnique();

            entity.Property(e => e.CompanyId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("company_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.BusinessName)
                .HasMaxLength(200)
                .HasColumnName("business_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.LogoUrl).HasColumnName("logo_url");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Ruc)
                .HasMaxLength(11)
                .IsFixedLength()
                .HasColumnName("ruc");
            entity.Property(e => e.TradeName)
                .HasMaxLength(200)
                .HasColumnName("trade_name");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customers_pkey");

            entity.ToTable("customers");

            entity.HasIndex(e => new { e.DocumentType, e.DocumentNumber }, "customers_document_key").IsUnique();

            entity.Property(e => e.CustomerId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("customer_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DocumentNumber)
                .HasMaxLength(20)
                .HasColumnName("document_number");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(10)
                .HasColumnName("document_type");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<DemandPrediction>(entity =>
        {
            entity.HasKey(e => e.DemandPredictionId).HasName("demand_predictions_pkey");

            entity.ToTable("demand_predictions");

            entity.HasIndex(e => new { e.ProductId, e.WarehouseId, e.PeriodStart, e.ModelVersion }, "demand_predictions_unique_key").IsUnique();

            entity.HasIndex(e => e.WarehouseId, "idx_demand_predictions_warehouse_id");

            entity.Property(e => e.DemandPredictionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("demand_prediction_id");
            entity.Property(e => e.ActualQuantity)
                .HasPrecision(12, 3)
                .HasColumnName("actual_quantity");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LowerBound)
                .HasPrecision(12, 3)
                .HasColumnName("lower_bound");
            entity.Property(e => e.ModelVersion)
                .HasMaxLength(50)
                .HasColumnName("model_version");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.PredictedQuantity)
                .HasPrecision(12, 3)
                .HasColumnName("predicted_quantity");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UpperBound)
                .HasPrecision(12, 3)
                .HasColumnName("upper_bound");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.Product).WithMany(p => p.DemandPredictions)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("demand_predictions_product_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.DemandPredictions)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("demand_predictions_warehouse_id_fkey");
        });

        modelBuilder.Entity<DocumentSeries>(entity =>
        {
            entity.HasKey(e => e.DocumentSeriesId).HasName("document_series_pkey");

            entity.ToTable("document_series");

            entity.HasIndex(e => e.Series, "document_series_series_key").IsUnique();

            entity.HasIndex(e => e.BranchId, "idx_document_series_branch_id");

            entity.Property(e => e.DocumentSeriesId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("document_series_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CurrentNumber).HasColumnName("current_number");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(10)
                .HasColumnName("document_type");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Series)
                .HasMaxLength(4)
                .HasColumnName("series");

            entity.HasOne(d => d.Branch).WithMany(p => p.DocumentSeries)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_series_branch_id_fkey");
        });

        modelBuilder.Entity<EmailOutbox>(entity =>
        {
            entity.HasKey(e => e.EmailOutboxId).HasName("email_outbox_pkey");

            entity.ToTable("email_outbox");

            entity.HasIndex(e => new { e.Status, e.ScheduledAt }, "idx_email_outbox_pending");

            entity.Property(e => e.EmailOutboxId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("email_outbox_id");
            entity.Property(e => e.Attempts).HasColumnName("attempts");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.HtmlBody).HasColumnName("html_body");
            entity.Property(e => e.LastError).HasColumnName("last_error");
            entity.Property(e => e.ProviderMessageId)
                .HasMaxLength(100)
                .HasColumnName("provider_message_id");
            entity.Property(e => e.ScheduledAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("scheduled_at");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(255)
                .HasColumnName("subject");
            entity.Property(e => e.Template)
                .HasMaxLength(50)
                .HasColumnName("template");
            entity.Property(e => e.ToEmail)
                .HasMaxLength(150)
                .HasColumnName("to_email");
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasKey(e => e.InventoryMovementId).HasName("inventory_movements_pkey");

            entity.ToTable("inventory_movements");

            entity.HasIndex(e => new { e.ProductId, e.WarehouseId, e.CreatedAt }, "idx_inventory_movements_kardex");

            entity.HasIndex(e => e.MovementTypeId, "idx_inventory_movements_movement_type_id");

            entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId }, "idx_inventory_movements_reference");

            entity.HasIndex(e => e.WarehouseId, "idx_inventory_movements_warehouse_id");

            entity.Property(e => e.InventoryMovementId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("inventory_movement_id");
            entity.Property(e => e.AverageCostAfter)
                .HasPrecision(14, 4)
                .HasColumnName("average_cost_after");
            entity.Property(e => e.BalanceAfter)
                .HasPrecision(12, 3)
                .HasColumnName("balance_after");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.MovementTypeId).HasColumnName("movement_type_id");
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(12, 3)
                .HasColumnName("quantity");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(30)
                .HasColumnName("reference_type");
            entity.Property(e => e.UnitCost)
                .HasPrecision(14, 4)
                .HasColumnName("unit_cost");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("inventory_movements_created_by_fkey");

            entity.HasOne(d => d.MovementType).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.MovementTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inventory_movements_movement_type_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inventory_movements_product_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inventory_movements_warehouse_id_fkey");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("invoices_pkey");

            entity.ToTable("invoices");

            entity.HasIndex(e => e.DocumentSeriesId, "idx_invoices_document_series_id");

            entity.HasIndex(e => e.IssuedAt, "idx_invoices_issued_at");

            entity.HasIndex(e => e.SaleId, "invoices_sale_id_key").IsUnique();

            entity.HasIndex(e => new { e.Series, e.Number }, "invoices_series_number_key").IsUnique();

            entity.Property(e => e.InvoiceId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("invoice_id");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'PEN'::bpchar")
                .IsFixedLength()
                .HasColumnName("currency");
            entity.Property(e => e.CustomerDocumentNumber)
                .HasMaxLength(20)
                .HasColumnName("customer_document_number");
            entity.Property(e => e.CustomerDocumentType)
                .HasMaxLength(10)
                .HasColumnName("customer_document_type");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(200)
                .HasColumnName("customer_name");
            entity.Property(e => e.DocumentSeriesId).HasColumnName("document_series_id");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(10)
                .HasColumnName("document_type");
            entity.Property(e => e.EmailedAt).HasColumnName("emailed_at");
            entity.Property(e => e.FullNumber)
                .HasMaxLength(13)
                .HasComputedColumnSql("(((series)::text || '-'::text) || lpad((number)::text, 8, '0'::text))", true)
                .HasColumnName("full_number");
            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("issued_at");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.PdfUrl).HasColumnName("pdf_url");
            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.Series)
                .HasMaxLength(4)
                .HasColumnName("series");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'ISSUED'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Subtotal)
                .HasPrecision(12, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(12, 2)
                .HasColumnName("tax_amount");
            entity.Property(e => e.Total)
                .HasPrecision(12, 2)
                .HasColumnName("total");

            entity.HasOne(d => d.DocumentSeries).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.DocumentSeriesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoices_document_series_id_fkey");

            entity.HasOne(d => d.Sale).WithOne(p => p.Invoice)
                .HasForeignKey<Invoice>(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoices_sale_id_fkey");
        });

        modelBuilder.Entity<MovementType>(entity =>
        {
            entity.HasKey(e => e.MovementTypeId).HasName("movement_types_pkey");

            entity.ToTable("movement_types");

            entity.HasIndex(e => e.Code, "movement_types_code_key").IsUnique();

            entity.Property(e => e.MovementTypeId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("movement_type_id");
            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");
            entity.Property(e => e.Direction).HasColumnName("direction");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.HasIndex(e => e.PaymentMethodId, "idx_payments_payment_method_id");

            entity.HasIndex(e => e.SaleId, "idx_payments_sale_id");

            entity.Property(e => e.PaymentId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.PaidAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethodId).HasColumnName("payment_method_id");
            entity.Property(e => e.Reference)
                .HasMaxLength(100)
                .HasColumnName("reference");
            entity.Property(e => e.SaleId).HasColumnName("sale_id");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payments_payment_method_id_fkey");

            entity.HasOne(d => d.Sale).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("payments_sale_id_fkey");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("payment_methods_pkey");

            entity.ToTable("payment_methods");

            entity.HasIndex(e => e.Code, "payment_methods_code_key").IsUnique();

            entity.Property(e => e.PaymentMethodId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("permissions_pkey");

            entity.ToTable("permissions");

            entity.HasIndex(e => e.Code, "permissions_code_key").IsUnique();

            entity.Property(e => e.PermissionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("permission_id");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .HasColumnName("module");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("products_pkey");

            entity.ToTable("products");

            entity.HasIndex(e => e.BrandId, "idx_products_brand_id");

            entity.HasIndex(e => e.CategoryId, "idx_products_category_id");

            entity.HasIndex(e => e.UnitId, "idx_products_unit_id");

            entity.HasIndex(e => e.Barcode, "products_barcode_key").IsUnique();

            entity.HasIndex(e => e.Sku, "products_sku_key").IsUnique();

            entity.Property(e => e.ProductId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("product_id");
            entity.Property(e => e.AverageCost)
                .HasPrecision(14, 4)
                .HasColumnName("average_cost");
            entity.Property(e => e.Barcode)
                .HasMaxLength(50)
                .HasColumnName("barcode");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsTaxable)
                .HasDefaultValue(true)
                .HasColumnName("is_taxable");
            entity.Property(e => e.MaxStock)
                .HasPrecision(12, 3)
                .HasColumnName("max_stock");
            entity.Property(e => e.MinStock)
                .HasPrecision(12, 3)
                .HasColumnName("min_stock");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("sku");
            entity.Property(e => e.UnitId).HasColumnName("unit_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("products_brand_id_fkey");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_category_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("products_created_by_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.Products)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_unit_id_fkey");
        });

        modelBuilder.Entity<ProductPrice>(entity =>
        {
            entity.HasKey(e => e.ProductPriceId).HasName("product_prices_pkey");

            entity.ToTable("product_prices");

            entity.HasIndex(e => new { e.ProductId, e.ValidTo }, "idx_product_prices_product_id");

            entity.Property(e => e.ProductPriceId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("product_price_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ValidFrom)
                .HasDefaultValueSql("now()")
                .HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("product_prices_created_by_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_prices_product_id_fkey");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.PurchaseOrderId).HasName("purchase_orders_pkey");

            entity.ToTable("purchase_orders");

            entity.HasIndex(e => e.Status, "idx_purchase_orders_status");

            entity.HasIndex(e => e.SupplierId, "idx_purchase_orders_supplier_id");

            entity.HasIndex(e => e.WarehouseId, "idx_purchase_orders_warehouse_id");

            entity.HasIndex(e => e.Code, "purchase_orders_code_key").IsUnique();

            entity.Property(e => e.PurchaseOrderId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("purchase_order_id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ExpectedDate).HasColumnName("expected_date");
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("order_date");
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Subtotal)
                .HasPrecision(12, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(12, 2)
                .HasColumnName("tax_amount");
            entity.Property(e => e.Total)
                .HasPrecision(12, 2)
                .HasColumnName("total");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.PurchaseOrderApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("purchase_orders_approved_by_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PurchaseOrderCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("purchase_orders_created_by_fkey");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("purchase_orders_supplier_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("purchase_orders_warehouse_id_fkey");
        });

        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasKey(e => e.PurchaseOrderDetailId).HasName("purchase_order_details_pkey");

            entity.ToTable("purchase_order_details");

            entity.HasIndex(e => e.ProductId, "idx_purchase_order_details_product_id");

            entity.HasIndex(e => new { e.PurchaseOrderId, e.ProductId }, "purchase_order_details_order_product_key").IsUnique();

            entity.Property(e => e.PurchaseOrderDetailId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("purchase_order_detail_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(12, 3)
                .HasColumnName("quantity");
            entity.Property(e => e.Subtotal)
                .HasPrecision(12, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.UnitCost)
                .HasPrecision(14, 4)
                .HasColumnName("unit_cost");

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("purchase_order_details_product_id_fkey");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.PurchaseOrderId)
                .HasConstraintName("purchase_order_details_purchase_order_id_fkey");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("refresh_tokens_pkey");

            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.UserId, "idx_refresh_tokens_user_id");

            entity.HasIndex(e => e.TokenHash, "refresh_tokens_token_hash_key").IsUnique();

            entity.Property(e => e.RefreshTokenId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("refresh_token_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByIp)
                .HasMaxLength(45)
                .HasColumnName("created_by_ip");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.TokenHash).HasColumnName("token_hash");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("refresh_tokens_user_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "roles_name_key").IsUnique();

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId }).HasName("role_permissions_pkey");

            entity.ToTable("role_permissions");

            entity.HasIndex(e => e.PermissionId, "idx_role_permissions_permission_id");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.GrantedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("granted_at");

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("role_permissions_permission_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("role_permissions_role_id_fkey");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("sales_pkey");

            entity.ToTable("sales");

            entity.HasIndex(e => e.BranchId, "idx_sales_branch_id");

            entity.HasIndex(e => e.CancelledBy, "idx_sales_cancelled_by");

            entity.HasIndex(e => e.CustomerId, "idx_sales_customer_id");

            entity.HasIndex(e => e.SellerId, "idx_sales_seller_id");

            entity.HasIndex(e => e.SoldAt, "idx_sales_sold_at");

            entity.HasIndex(e => e.WarehouseId, "idx_sales_warehouse_id");

            entity.Property(e => e.SaleId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("sale_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CancelReason)
                .HasMaxLength(255)
                .HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DiscountTotal)
                .HasPrecision(12, 2)
                .HasColumnName("discount_total");
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.SoldAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("sold_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'COMPLETED'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Subtotal)
                .HasPrecision(12, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(12, 2)
                .HasColumnName("tax_amount");
            entity.Property(e => e.Total)
                .HasPrecision(12, 2)
                .HasColumnName("total");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.Branch).WithMany(p => p.Sales)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_branch_id_fkey");

            entity.HasOne(d => d.CancelledByNavigation).WithMany(p => p.SaleCancelledByNavigations)
                .HasForeignKey(d => d.CancelledBy)
                .HasConstraintName("sales_cancelled_by_fkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_customer_id_fkey");

            entity.HasOne(d => d.Seller).WithMany(p => p.SaleSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_seller_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Sales)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_warehouse_id_fkey");
        });

        modelBuilder.Entity<SaleDetail>(entity =>
        {
            entity.HasKey(e => e.SaleDetailId).HasName("sale_details_pkey");

            entity.ToTable("sale_details");

            entity.HasIndex(e => e.ProductId, "idx_sale_details_product_id");

            entity.HasIndex(e => new { e.SaleId, e.ProductId }, "sale_details_sale_product_key").IsUnique();

            entity.Property(e => e.SaleDetailId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("sale_detail_id");
            entity.Property(e => e.Discount)
                .HasPrecision(12, 2)
                .HasColumnName("discount");
            entity.Property(e => e.LineTotal)
                .HasPrecision(12, 2)
                .HasColumnName("line_total");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(12, 3)
                .HasColumnName("quantity");
            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(12, 2)
                .HasColumnName("tax_amount");
            entity.Property(e => e.TaxRate)
                .HasPrecision(5, 4)
                .HasDefaultValue(0.18m)
                .HasColumnName("tax_rate");
            entity.Property(e => e.UnitCost)
                .HasPrecision(14, 4)
                .HasColumnName("unit_cost");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(12, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Product).WithMany(p => p.SaleDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_details_product_id_fkey");

            entity.HasOne(d => d.Sale).WithMany(p => p.SaleDetails)
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("sale_details_sale_id_fkey");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.StockId).HasName("stocks_pkey");

            entity.ToTable("stocks");

            entity.HasIndex(e => e.WarehouseId, "idx_stocks_warehouse_id");

            entity.HasIndex(e => new { e.ProductId, e.WarehouseId }, "stocks_product_warehouse_key").IsUnique();

            entity.Property(e => e.StockId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("stock_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(12, 3)
                .HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.Product).WithMany(p => p.Stocks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stocks_product_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Stocks)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stocks_warehouse_id_fkey");
        });

        modelBuilder.Entity<StockAlert>(entity =>
        {
            entity.HasKey(e => e.StockAlertId).HasName("stock_alerts_pkey");

            entity.ToTable("stock_alerts");

            entity.HasIndex(e => e.DemandPredictionId, "idx_stock_alerts_demand_prediction_id");

            entity.HasIndex(e => new { e.ProductId, e.WarehouseId }, "idx_stock_alerts_product_warehouse");

            entity.HasIndex(e => e.Status, "idx_stock_alerts_status");

            entity.HasIndex(e => e.WarehouseId, "idx_stock_alerts_warehouse_id");

            entity.Property(e => e.StockAlertId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("stock_alert_id");
            entity.Property(e => e.AlertType)
                .HasMaxLength(30)
                .HasColumnName("alert_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentStock)
                .HasPrecision(12, 3)
                .HasColumnName("current_stock");
            entity.Property(e => e.DemandPredictionId).HasColumnName("demand_prediction_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy).HasColumnName("resolved_by");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'OPEN'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SuggestedOrderQty)
                .HasPrecision(12, 3)
                .HasColumnName("suggested_order_qty");
            entity.Property(e => e.Threshold)
                .HasPrecision(12, 3)
                .HasColumnName("threshold");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.DemandPrediction).WithMany(p => p.StockAlerts)
                .HasForeignKey(d => d.DemandPredictionId)
                .HasConstraintName("stock_alerts_demand_prediction_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.StockAlerts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_alerts_product_id_fkey");

            entity.HasOne(d => d.ResolvedByNavigation).WithMany(p => p.StockAlerts)
                .HasForeignKey(d => d.ResolvedBy)
                .HasConstraintName("stock_alerts_resolved_by_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockAlerts)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_alerts_warehouse_id_fkey");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("suppliers_pkey");

            entity.ToTable("suppliers");

            entity.HasIndex(e => e.Ruc, "suppliers_ruc_key").IsUnique();

            entity.Property(e => e.SupplierId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("supplier_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.BusinessName)
                .HasMaxLength(200)
                .HasColumnName("business_name");
            entity.Property(e => e.ContactName)
                .HasMaxLength(150)
                .HasColumnName("contact_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Ruc)
                .HasMaxLength(11)
                .IsFixedLength()
                .HasColumnName("ruc");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.HasKey(e => e.TransferId).HasName("transfers_pkey");

            entity.ToTable("transfers");

            entity.HasIndex(e => e.DestinationWarehouseId, "idx_transfers_destination_warehouse_id");

            entity.HasIndex(e => e.OriginWarehouseId, "idx_transfers_origin_warehouse_id");

            entity.HasIndex(e => e.Status, "idx_transfers_status");

            entity.HasIndex(e => e.Code, "transfers_code_key").IsUnique();

            entity.Property(e => e.TransferId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("transfer_id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CompletedBy).HasColumnName("completed_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DestinationWarehouseId).HasColumnName("destination_warehouse_id");
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");
            entity.Property(e => e.OriginWarehouseId).HasColumnName("origin_warehouse_id");
            entity.Property(e => e.RequestedBy).HasColumnName("requested_by");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.CompletedByNavigation).WithMany(p => p.TransferCompletedByNavigations)
                .HasForeignKey(d => d.CompletedBy)
                .HasConstraintName("transfers_completed_by_fkey");

            entity.HasOne(d => d.DestinationWarehouse).WithMany(p => p.TransferDestinationWarehouses)
                .HasForeignKey(d => d.DestinationWarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transfers_destination_warehouse_id_fkey");

            entity.HasOne(d => d.OriginWarehouse).WithMany(p => p.TransferOriginWarehouses)
                .HasForeignKey(d => d.OriginWarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transfers_origin_warehouse_id_fkey");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.TransferRequestedByNavigations)
                .HasForeignKey(d => d.RequestedBy)
                .HasConstraintName("transfers_requested_by_fkey");
        });

        modelBuilder.Entity<TransferDetail>(entity =>
        {
            entity.HasKey(e => e.TransferDetailId).HasName("transfer_details_pkey");

            entity.ToTable("transfer_details");

            entity.HasIndex(e => e.ProductId, "idx_transfer_details_product_id");

            entity.HasIndex(e => new { e.TransferId, e.ProductId }, "transfer_details_transfer_product_key").IsUnique();

            entity.Property(e => e.TransferDetailId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("transfer_detail_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(12, 3)
                .HasColumnName("quantity");
            entity.Property(e => e.TransferId).HasColumnName("transfer_id");

            entity.HasOne(d => d.Product).WithMany(p => p.TransferDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transfer_details_product_id_fkey");

            entity.HasOne(d => d.Transfer).WithMany(p => p.TransferDetails)
                .HasForeignKey(d => d.TransferId)
                .HasConstraintName("transfer_details_transfer_id_fkey");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.UnitId).HasName("units_pkey");

            entity.ToTable("units");

            entity.HasIndex(e => e.Code, "units_code_key").IsUnique();

            entity.Property(e => e.UnitId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("unit_id");
            entity.Property(e => e.AllowsDecimals).HasColumnName("allows_decimals");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.BranchId, "idx_users_branch_id");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("user_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.EmailConfirmed).HasColumnName("email_confirmed");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Branch).WithMany(p => p.Users)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("users_branch_id_fkey");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("user_roles_pkey");

            entity.ToTable("user_roles");

            entity.HasIndex(e => e.RoleId, "idx_user_roles_role_id");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("assigned_at");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("user_roles_role_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_roles_user_id_fkey");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId).HasName("warehouses_pkey");

            entity.ToTable("warehouses");

            entity.HasIndex(e => e.BranchId, "idx_warehouses_branch_id");

            entity.HasIndex(e => e.Code, "warehouses_code_key").IsUnique();

            entity.Property(e => e.WarehouseId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("warehouse_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Branch).WithMany(p => p.Warehouses)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("warehouses_branch_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
