namespace Magazing.Application.DTOs.Catalog;

public static class ProductImportColumns
{
    public const string Sku = "sku";
    public const string Name = "nombre";
    public const string Category = "categoria";
    public const string Unit = "unidad";
    public const string Price = "precio";
    public const string Brand = "marca";
    public const string Barcode = "codigo_barras";
    public const string MinStock = "stock_minimo";
    public const string MaxStock = "stock_maximo";
    public const string Taxable = "afecto_igv";
    public const string Description = "descripcion";

    public static readonly string[] All = [Sku, Name, Category, Unit, Price, Brand, Barcode, MinStock, MaxStock, Taxable, Description];

    public static readonly string[] Required = [Sku, Name, Category, Unit, Price];
}

public static class ProductImportStatuses
{
    public const string Valid = "valid";
    public const string Error = "error";
    public const string Created = "created";
}

public class ProductImportRowDto
{
    public int Row { get; set; }

    public string? Sku { get; set; }

    public string? Name { get; set; }

    public string Status { get; set; } = ProductImportStatuses.Valid;

    public string? Message { get; set; }
}

public class ProductImportResultDto
{
    public bool DryRun { get; set; }

    public int TotalRows { get; set; }

    public int ValidRows { get; set; }

    public int ErrorRows { get; set; }

    public int Created { get; set; }

    public List<ProductImportRowDto> Rows { get; set; } = [];
}

public sealed record ProductImportCatalogs(
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> Brands,
    IReadOnlyList<(string Code, string Name)> Units);
