using Magazing.Application.DTOs.Catalog;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;
using Unit = Magazing.Domain.Entities.Unit;

namespace Magazing.Application.UseCases.Products.Commands;

public record ImportProductsCommand(Stream Content, string FileName, bool DryRun, Guid CreatedBy) : IRequest<ProductImportResultDto>;

internal sealed class ImportProductsCommandHandler(IUnitOfWork unitOfWork, ISpreadsheetReader spreadsheetReader)
    : IRequestHandler<ImportProductsCommand, ProductImportResultDto>
{
    public async Task<ProductImportResultDto> Handle(ImportProductsCommand request, CancellationToken cancellationToken)
    {
        var rows = spreadsheetReader.Read(request.Content, request.FileName);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("El archivo no tiene productos: llena la plantilla desde la fila 2.");
        }

        var missing = ProductImportColumns.Required.Where(column => !rows[0].Cells.ContainsKey(column)).ToList();
        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"Faltan columnas obligatorias: {string.Join(", ", missing)}. Usa la plantilla.");
        }

        var categories = (await unitOfWork.Repository<Category>().FindAsync(item => item.IsActive, cancellationToken))
            .GroupBy(item => ImportText.Key(item.Name)).ToDictionary(group => group.Key, group => group.First().CategoryId);
        var brands = (await unitOfWork.Repository<Brand>().FindAsync(item => item.IsActive, cancellationToken))
            .GroupBy(item => ImportText.Key(item.Name)).ToDictionary(group => group.Key, group => group.First().BrandId);
        var units = (await unitOfWork.Repository<Unit>().FindAsync(item => true, cancellationToken)).ToList();
        var unitKeys = units.Select(item => (Key: ImportText.Key(item.Code), item.UnitId))
            .Concat(units.Select(item => (Key: ImportText.Key(item.Name), item.UnitId)))
            .GroupBy(item => item.Key).ToDictionary(group => group.Key, group => group.First().UnitId);

        var skus = rows.Select(row => ProductFactory.NormalizeSku(Cell(row, ProductImportColumns.Sku))).Where(sku => sku.Length > 0).ToList();
        var barcodes = rows.Select(row => ProductFactory.NormalizeBarcode(Cell(row, ProductImportColumns.Barcode))).OfType<string>().ToList();
        var products = unitOfWork.Repository<Product>();
        var existingSkus = (await products.FindAsync(item => skus.Contains(item.Sku), cancellationToken)).Select(item => item.Sku).ToHashSet();
        var existingBarcodes = (await products.FindAsync(item => item.Barcode != null && barcodes.Contains(item.Barcode), cancellationToken))
            .Select(item => item.Barcode!).ToHashSet();

        var result = new ProductImportResultDto { DryRun = request.DryRun, TotalRows = rows.Count };
        var valid = new List<(ProductImportRowDto Row, NewProduct Product)>();
        var seenSkus = new HashSet<string>();
        var seenBarcodes = new HashSet<string>();

        foreach (var row in rows)
        {
            var sku = ProductFactory.NormalizeSku(Cell(row, ProductImportColumns.Sku));
            var name = Cell(row, ProductImportColumns.Name);
            var line = new ProductImportRowDto { Row = row.Number, Sku = sku, Name = name };
            result.Rows.Add(line);

            try
            {
                var product = ParseRow(row, sku, name, categories, brands, unitKeys);

                if (existingSkus.Contains(sku))
                {
                    throw new InvalidOperationException("Ya existe un producto con ese SKU.");
                }

                if (!seenSkus.Add(sku))
                {
                    throw new InvalidOperationException("El SKU se repite en el archivo.");
                }

                if (product.Barcode is { } barcode && (existingBarcodes.Contains(barcode) || !seenBarcodes.Add(barcode)))
                {
                    throw new InvalidOperationException("El código de barras ya existe o se repite en el archivo.");
                }

                valid.Add((line, product));
            }
            catch (Exception ex) when (ex is InvalidOperationException or FormatException)
            {
                line.Status = ProductImportStatuses.Error;
                line.Message = ex.Message;
            }
        }

        result.ValidRows = valid.Count;
        result.ErrorRows = result.Rows.Count - valid.Count;

        if (request.DryRun || valid.Count == 0)
        {
            return result;
        }

        foreach (var (line, product) in valid)
        {
            await ProductFactory.AddAsync(unitOfWork, product, request.CreatedBy, cancellationToken);
            line.Status = ProductImportStatuses.Created;
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        result.Created = valid.Count;

        return result;
    }

    private static NewProduct ParseRow(
        SpreadsheetRow row,
        string sku,
        string name,
        IReadOnlyDictionary<string, Guid> categories,
        IReadOnlyDictionary<string, Guid> brands,
        IReadOnlyDictionary<string, Guid> units)
    {
        if (sku.Length == 0)
        {
            throw new InvalidOperationException("Falta el SKU.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Falta el nombre.");
        }

        var categoryText = Cell(row, ProductImportColumns.Category);
        if (!categories.TryGetValue(ImportText.Key(categoryText), out var categoryId))
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(categoryText)
                ? "Falta la categoría."
                : $"La categoría \"{categoryText}\" no existe o está inactiva.");
        }

        var unitText = Cell(row, ProductImportColumns.Unit);
        if (!units.TryGetValue(ImportText.Key(unitText), out var unitId))
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(unitText)
                ? "Falta la unidad."
                : $"La unidad \"{unitText}\" no existe.");
        }

        Guid? brandId = null;
        var brandText = Cell(row, ProductImportColumns.Brand);
        if (!string.IsNullOrWhiteSpace(brandText))
        {
            brandId = brands.TryGetValue(ImportText.Key(brandText), out var id)
                ? id
                : throw new InvalidOperationException($"La marca \"{brandText}\" no existe. Créala primero o deja la celda vacía.");
        }

        var price = ImportText.ParseDecimal(Cell(row, ProductImportColumns.Price))
            ?? throw new InvalidOperationException("Falta el precio.");
        ProductReadModel.ValidatePrice(price);

        var minStock = ImportText.ParseDecimal(Cell(row, ProductImportColumns.MinStock)) ?? 0;
        var maxStock = ImportText.ParseDecimal(Cell(row, ProductImportColumns.MaxStock));
        if (minStock < 0)
        {
            throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
        }

        if (maxStock is { } max && max < minStock)
        {
            throw new InvalidOperationException("El stock máximo no puede ser menor que el mínimo.");
        }

        return new NewProduct(
            sku,
            ProductFactory.NormalizeBarcode(Cell(row, ProductImportColumns.Barcode)),
            name,
            Cell(row, ProductImportColumns.Description),
            categoryId,
            brandId,
            unitId,
            ImportText.ParseBool(Cell(row, ProductImportColumns.Taxable), true),
            minStock,
            maxStock,
            price,
            null);
    }

    private static string Cell(SpreadsheetRow row, string column)
    {
        return row.Cells.TryGetValue(column, out var value) ? value : string.Empty;
    }
}
