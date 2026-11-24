using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using Magazing.Application.DTOs.Assistant;
using Magazing.Application.Services;
using Magazing.Application.Services.Common;
using Magazing.Application.Services.Security;
using Magazing.Application.UseCases.Alerts.Queries;
using Magazing.Application.UseCases.Brands.Queries;
using Magazing.Application.UseCases.Categories.Queries;
using Magazing.Application.UseCases.Products.Queries;
using Magazing.Application.UseCases.Reports.Queries;
using Magazing.Application.UseCases.Stocks.Queries;
using Magazing.Application.UseCases.Suppliers.Queries;
using Magazing.Application.UseCases.Units.Queries;
using Magazing.Application.UseCases.Warehouses.Queries;
using MediatR;

namespace Magazing.Application.UseCases.Assistant;

internal sealed record AssistantTool(string Name, string Description, string Schema, string Permission, bool IsProposal);

internal sealed record ToolResult(string Content, AssistantProposalDto? Proposal = null);

internal sealed class AssistantTools(ISender sender)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private static readonly CultureInfo Peru = CultureInfo.GetCultureInfo("es-PE");

    public static readonly IReadOnlyList<AssistantTool> All =
    [
        new("buscar_productos",
            "Busca productos activos por nombre, SKU o código de barras. Sin texto lista los productos activos. Devuelve id, sku, nombre, categoría, unidad, precio de venta y costo promedio.",
            """{"type":"object","properties":{"texto":{"type":"string","description":"Parte del nombre, SKU o código de barras (opcional)"}}}""",
            Permissions.CatalogRead, false),
        new("consultar_stock",
            "Consulta el stock por almacén. Sin texto lista los productos de cada almacén; con texto filtra por nombre o SKU.",
            """{"type":"object","properties":{"texto":{"type":"string","description":"Producto a consultar (opcional)"},"estado":{"type":"string","enum":["OK","LOW","OUT"],"description":"LOW = bajo mínimo, OUT = sin stock (opcional)"}}}""",
            Permissions.InventoryRead, false),
        new("buscar_proveedores",
            "Busca proveedores activos por razón social o RUC. Sin texto lista los proveedores activos.",
            """{"type":"object","properties":{"texto":{"type":"string","description":"Razón social o RUC (opcional)"}}}""",
            Permissions.PurchasesRead, false),
        new("resumen_ventas",
            "Total vendido, cantidad de comprobantes, costo y utilidad bruta entre dos fechas (formato YYYY-MM-DD, hora de Lima).",
            """{"type":"object","properties":{"desde":{"type":"string"},"hasta":{"type":"string"}},"required":["desde","hasta"]}""",
            Permissions.ReportsRead, false),
        new("alertas_abiertas",
            "Alertas de stock abiertas: productos bajo mínimo o con faltante según la predicción de demanda.",
            """{"type":"object","properties":{}}""",
            Permissions.AlertsManage, false),
        new("listar_catalogos",
            "Lista las categorías, marcas, unidades de medida y almacenes disponibles, para elegir valores válidos.",
            """{"type":"object","properties":{}}""",
            Permissions.CatalogRead, false),
        new("proponer_producto",
            "Prepara la creación de un producto. NO lo guarda: el usuario debe confirmarlo en pantalla. Usa nombres de categoría, marca y unidad tal como existen.",
            """{"type":"object","properties":{"nombre":{"type":"string"},"sku":{"type":"string","description":"Código interno único, en mayúsculas"},"categoria":{"type":"string"},"unidad":{"type":"string","description":"Código o nombre de la unidad, p. ej. NIU o Unidad"},"marca":{"type":"string"},"precio":{"type":"number","description":"Precio de venta con IGV"},"stock_minimo":{"type":"number"},"afecto_igv":{"type":"boolean"}},"required":["nombre","sku","categoria","unidad","precio"]}""",
            Permissions.CatalogManage, true),
        new("proponer_orden_compra",
            "Prepara una orden de compra en borrador. NO la guarda: el usuario debe confirmarla en pantalla.",
            """{"type":"object","properties":{"proveedor":{"type":"string","description":"Razón social o RUC"},"almacen":{"type":"string","description":"Código o nombre del almacén de destino (opcional)"},"notas":{"type":"string"},"items":{"type":"array","items":{"type":"object","properties":{"producto":{"type":"string","description":"SKU o nombre del producto"},"cantidad":{"type":"number"},"costo_unitario":{"type":"number","description":"Costo unitario sin IGV"}},"required":["producto","cantidad","costo_unitario"]}}},"required":["proveedor","items"]}""",
            Permissions.PurchasesCreate, true),
    ];

    public static IReadOnlyList<AssistantTool> For(IReadOnlyCollection<string> permissions)
    {
        return All.Where(tool => permissions.Contains(tool.Permission)).ToList();
    }

    public async Task<ToolResult> ExecuteAsync(string name, string argumentsJson, CancellationToken cancellationToken)
    {
        JsonElement args;
        try
        {
            args = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson).RootElement.Clone();
        }
        catch (JsonException)
        {
            return new ToolResult("""{"error":"Argumentos inválidos."}""");
        }

        try
        {
            return name switch
            {
                "buscar_productos" => await SearchProductsAsync(Text(args, "texto"), cancellationToken),
                "consultar_stock" => await StockAsync(Text(args, "texto"), Text(args, "estado"), cancellationToken),
                "buscar_proveedores" => await SearchSuppliersAsync(Text(args, "texto"), cancellationToken),
                "resumen_ventas" => await SalesSummaryAsync(Text(args, "desde"), Text(args, "hasta"), cancellationToken),
                "alertas_abiertas" => await OpenAlertsAsync(cancellationToken),
                "listar_catalogos" => await CatalogsAsync(cancellationToken),
                "proponer_producto" => await ProposeProductAsync(args, cancellationToken),
                "proponer_orden_compra" => await ProposePurchaseOrderAsync(args, cancellationToken),
                _ => new ToolResult("""{"error":"Herramienta desconocida."}""")
            };
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or FormatException)
        {
            return new ToolResult(Serialize(new { error = ex.Message }));
        }
    }

    private async Task<ToolResult> SearchProductsAsync(string? text, CancellationToken cancellationToken)
    {
        var page = await sender.Send(new GetProductsQuery(1, 10, text, null, null, true), cancellationToken);
        return new ToolResult(Serialize(page.Items.Select(p => new
        {
            id = p.ProductId, p.Sku, nombre = p.Name, categoria = p.CategoryName, marca = p.BrandName,
            unidad = p.UnitCode, precio = p.CurrentPrice, costoPromedio = p.AverageCost
        })));
    }

    private async Task<ToolResult> StockAsync(string? text, string? status, CancellationToken cancellationToken)
    {
        var warehouses = (await sender.Send(new GetWarehousesQuery(true), cancellationToken)).Where(w => w.IsActive).ToList();
        var result = new List<object>();
        foreach (var warehouse in warehouses)
        {
            var page = await sender.Send(new GetStocksQuery(warehouse.WarehouseId, status, text, 1, 15), cancellationToken);
            result.Add(new
            {
                almacen = $"{warehouse.Code} · {warehouse.Name}",
                productos = page.Items.Select(s => new
                {
                    s.Sku, nombre = s.ProductName, cantidad = s.Quantity, unidad = s.UnitCode, minimo = s.MinStock, estado = s.Status
                }),
                total = page.TotalCount
            });
        }

        return new ToolResult(Serialize(result));
    }

    private async Task<ToolResult> SearchSuppliersAsync(string? text, CancellationToken cancellationToken)
    {
        var page = await sender.Send(new GetSuppliersQuery(1, 10, text, true), cancellationToken);
        return new ToolResult(Serialize(page.Items.Select(s => new { id = s.SupplierId, s.Ruc, razonSocial = s.BusinessName, contacto = s.ContactName })));
    }

    private async Task<ToolResult> SalesSummaryAsync(string? from, string? to, CancellationToken cancellationToken)
    {
        var start = DateOnly.ParseExact(from ?? string.Empty, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var end = DateOnly.ParseExact(to ?? string.Empty, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var report = await sender.Send(new GetSalesReportQuery(start, end, "day"), cancellationToken);
        return new ToolResult(Serialize(new { desde = from, hasta = to, report.Totals }));
    }

    private async Task<ToolResult> OpenAlertsAsync(CancellationToken cancellationToken)
    {
        var alerts = await sender.Send(new GetAlertsQuery("OPEN"), cancellationToken);
        return new ToolResult(Serialize(alerts.Take(20).Select(a => new
        {
            a.Sku, producto = a.ProductName, almacen = a.WarehouseName, tipo = a.AlertType,
            stockActual = a.CurrentStock, umbral = a.Threshold, sugeridoComprar = a.SuggestedOrderQty
        })));
    }

    private async Task<ToolResult> CatalogsAsync(CancellationToken cancellationToken)
    {
        var categories = await sender.Send(new GetCategoriesQuery(true), cancellationToken);
        var brands = await sender.Send(new GetBrandsQuery(true), cancellationToken);
        var units = await sender.Send(new GetUnitsQuery(), cancellationToken);
        var warehouses = await sender.Send(new GetWarehousesQuery(true), cancellationToken);
        return new ToolResult(Serialize(new
        {
            categorias = categories.Select(c => c.Name),
            marcas = brands.Select(b => b.Name),
            unidades = units.Select(u => new { u.Code, nombre = u.Name }),
            almacenes = warehouses.Where(w => w.IsActive).Select(w => new { w.Code, nombre = w.Name })
        }));
    }

    private async Task<ToolResult> ProposeProductAsync(JsonElement args, CancellationToken cancellationToken)
    {
        var name = Required(args, "nombre");
        var sku = Required(args, "sku").ToUpperInvariant();
        var price = Number(args, "precio") ?? throw new InvalidOperationException("Falta el precio.");
        var minStock = Number(args, "stock_minimo") ?? 0;
        var isTaxable = args.TryGetProperty("afecto_igv", out var taxable) && taxable.ValueKind == JsonValueKind.False ? false : true;

        var categories = await sender.Send(new GetCategoriesQuery(true), cancellationToken);
        var category = PickOne(categories, Required(args, "categoria"), c => c.Name, "ninguna categoría", "categorías");

        var units = await sender.Send(new GetUnitsQuery(), cancellationToken);
        var unitText = Required(args, "unidad");
        var unit = units.FirstOrDefault(u => u.Code.Equals(unitText, StringComparison.OrdinalIgnoreCase))
            ?? PickOne(units, unitText, u => u.Name, "ninguna unidad", "unidades");

        var brandText = Text(args, "marca");
        var brand = string.IsNullOrWhiteSpace(brandText)
            ? null
            : PickOne(await sender.Send(new GetBrandsQuery(true), cancellationToken), brandText, b => b.Name, "ninguna marca", "marcas");

        var existing = await sender.Send(new GetProductsQuery(1, 5, sku, null, null, null), cancellationToken);
        if (existing.Items.Any(p => p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Ya existe un producto con el SKU {sku}.");
        }

        var request = new
        {
            sku, barcode = (string?)null, name, description = (string?)null, categoryId = category.CategoryId,
            brandId = brand?.BrandId, unitId = unit.UnitId, isTaxable, minStock, maxStock = (decimal?)null,
            price, imageUrl = (string?)null
        };

        var proposal = new AssistantProposalDto
        {
            Kind = "create_product",
            Title = "Nuevo producto",
            Lines =
            [
                $"{name} · SKU {sku}",
                $"Categoría: {category.Name}" + (brand is null ? string.Empty : $" · Marca: {brand.Name}"),
                $"Unidad: {unit.Code} · Stock mínimo: {minStock.ToString("0.##", Peru)}",
                $"Precio de venta: {Money(price)}" + (isTaxable ? " (incluye IGV)" : " (no afecto a IGV)")
            ],
            Request = JsonSerializer.SerializeToElement(request, Json)
        };

        return new ToolResult("""{"estado":"Propuesta lista. Se muestra al usuario para que la confirme; todavía no se guardó."}""", proposal);
    }

    private async Task<ToolResult> ProposePurchaseOrderAsync(JsonElement args, CancellationToken cancellationToken)
    {
        var suppliers = await sender.Send(new GetSuppliersQuery(1, 10, Required(args, "proveedor"), true), cancellationToken);
        var supplier = suppliers.Items.Count == 1
            ? suppliers.Items[0]
            : PickOne(suppliers.Items, Required(args, "proveedor"), s => s.BusinessName, "ningún proveedor", "proveedores");

        var warehouses = (await sender.Send(new GetWarehousesQuery(true), cancellationToken)).Where(w => w.IsActive).OrderBy(w => w.Code).ToList();
        var warehouseText = Text(args, "almacen");
        var warehouse = string.IsNullOrWhiteSpace(warehouseText)
            ? warehouses.FirstOrDefault() ?? throw new InvalidOperationException("No hay almacenes activos.")
            : warehouses.FirstOrDefault(w => w.Code.Equals(warehouseText, StringComparison.OrdinalIgnoreCase))
              ?? PickOne(warehouses, warehouseText, w => w.Name, "ningún almacén", "almacenes");

        if (!args.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array || itemsElement.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("La orden necesita al menos un producto.");
        }

        var items = new List<object>();
        var lines = new List<string> { $"Proveedor: {supplier.BusinessName} (RUC {supplier.Ruc})", $"Almacén de destino: {warehouse.Code} · {warehouse.Name}" };
        decimal total = 0;

        foreach (var item in itemsElement.EnumerateArray())
        {
            var productText = Required(item, "producto");
            var quantity = Number(item, "cantidad") ?? 0;
            var unitCost = Number(item, "costo_unitario") ?? -1;
            if (quantity <= 0 || unitCost < 0)
            {
                throw new InvalidOperationException($"Cantidad o costo inválido para {productText}.");
            }

            var found = await sender.Send(new GetProductsQuery(1, 10, productText, null, null, true), cancellationToken);
            var product = found.Items.FirstOrDefault(p => p.Sku.Equals(productText, StringComparison.OrdinalIgnoreCase))
                ?? PickOne(found.Items, productText, p => p.Name, "ningún producto", "productos");

            items.Add(new { productId = product.ProductId, quantity, unitCost });
            lines.Add($"{quantity.ToString("0.###", Peru)} {product.UnitCode} × {product.Name} a {Money(unitCost)}");
            total += quantity * unitCost;
        }

        lines.Add($"Total estimado sin IGV: {Money(total)}");

        var request = new
        {
            supplierId = supplier.SupplierId,
            warehouseId = warehouse.WarehouseId,
            expectedDate = (string?)null,
            notes = Text(args, "notas") ?? "Creada con el asistente",
            items
        };

        var proposal = new AssistantProposalDto
        {
            Kind = "create_purchase_order",
            Title = "Nueva orden de compra (borrador)",
            Lines = lines,
            Request = JsonSerializer.SerializeToElement(request, Json)
        };

        return new ToolResult("""{"estado":"Propuesta lista. Se muestra al usuario para que la confirme; todavía no se guardó."}""", proposal);
    }

    private static T PickOne<T>(IEnumerable<T> source, string text, Func<T, string> name, string label, string plural)
    {
        var list = source.ToList();
        var exact = list.Where(x => name(x).Equals(text, StringComparison.OrdinalIgnoreCase)).ToList();
        if (exact.Count == 1)
        {
            return exact[0];
        }

        var partial = list.Where(x => name(x).Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();
        return partial.Count switch
        {
            1 => partial[0],
            0 => throw new InvalidOperationException($"No encontré {label} que coincida con \"{text}\"."),
            _ => throw new InvalidOperationException(
                $"Hay varios {plural} que coinciden con \"{text}\": {string.Join(", ", partial.Take(6).Select(name))}. Pregunta al usuario cuál es.")
        };
    }

    private static string Required(JsonElement args, string property)
    {
        var value = Text(args, property);
        return string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException($"Falta el dato \"{property}\".") : value.Trim();
    }

    private static string? Text(JsonElement args, string property)
    {
        return args.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static decimal? Number(JsonElement args, string property)
    {
        if (!args.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetDecimal(),
            JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null
        };
    }

    private static string Money(decimal amount)
    {
        return "S/ " + amount.ToString("N2", Peru);
    }

    private static string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, Json);
    }

    public static string Today()
    {
        return BusinessTime.Today().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
