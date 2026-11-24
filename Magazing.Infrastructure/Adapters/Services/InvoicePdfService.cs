using System.Globalization;
using Magazing.Application.Services.Common;
using Magazing.Application.DTOs.Sales;
using Magazing.Application.DTOs.Settings;
using Magazing.Application.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Magazing.Infrastructure.Adapters.Services;

public class InvoicePdfService : IInvoicePdfService
{
    private static readonly CultureInfo Peru = CultureInfo.GetCultureInfo("es-PE");
    private const string Accent = "#18181B";
    private const string Muted = "#71717A";
    private const string Border = "#E4E4E7";

    static InvoicePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] BuildInvoice(CompanyDto company, SaleDetailDto sale)
    {
        var title = sale.DocumentType == "FACTURA" ? "FACTURA DE VENTA" : "BOLETA DE VENTA";
        var cancelled = sale.Status == "CANCELLED";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(style => style.FontSize(9).FontColor(Accent));

                page.Header().Element(header => ComposeHeader(header, company, title, sale.FullNumber));
                page.Content().PaddingVertical(14).Element(content => ComposeContent(content, sale));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Representación impresa de un comprobante de prueba generado por Magazing. No tiene validez tributaria.")
                        .FontSize(7).FontColor(Muted);
                });

                if (cancelled)
                {
                    page.Foreground().AlignCenter().AlignMiddle().Rotate(-30)
                        .Text("ANULADO").FontSize(90).Bold().FontColor("#40DC2626");
                }
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, CompanyDto company, string title, string fullNumber)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(company.TradeName ?? company.BusinessName).FontSize(18).Bold();
                column.Item().Text(company.BusinessName).FontSize(9).FontColor(Muted);
                if (!string.IsNullOrWhiteSpace(company.Address))
                {
                    column.Item().Text(company.Address).FontColor(Muted);
                }

                if (!string.IsNullOrWhiteSpace(company.Phone) || !string.IsNullOrWhiteSpace(company.Email))
                {
                    column.Item().Text(string.Join(" · ", new[] { company.Phone, company.Email }.Where(value => !string.IsNullOrWhiteSpace(value))))
                        .FontColor(Muted);
                }
            });

            row.ConstantItem(190).Border(1.5f).BorderColor(Accent).Padding(10).Column(column =>
            {
                column.Item().AlignCenter().Text($"RUC {company.Ruc}").Bold().FontSize(11);
                column.Item().AlignCenter().PaddingVertical(4).Text(title).Bold().FontSize(11);
                column.Item().AlignCenter().Text(fullNumber).FontSize(12).Bold();
            });
        });
    }

    private static void ComposeContent(IContainer container, SaleDetailDto sale)
    {
        container.Column(column =>
        {
            column.Spacing(12);

            column.Item().Border(1).BorderColor(Border).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(info =>
                {
                    info.Item().Text(text =>
                    {
                        text.Span("Cliente: ").Bold();
                        text.Span(sale.CustomerName);
                    });
                    info.Item().Text(text =>
                    {
                        text.Span($"{sale.CustomerDocumentType}: ").Bold();
                        text.Span(sale.CustomerDocumentNumber);
                    });
                });

                row.RelativeItem().AlignRight().Column(info =>
                {
                    info.Item().AlignRight().Text(text =>
                    {
                        text.Span("Fecha de emision: ").Bold();
                        text.Span($"{BusinessTime.ToLima(sale.SoldAt):dd/MM/yyyy HH:mm}");
                    });
                    info.Item().AlignRight().Text(text =>
                    {
                        text.Span("Moneda: ").Bold();
                        text.Span("Soles (PEN)");
                    });
                    info.Item().AlignRight().Text(text =>
                    {
                        text.Span("Vendedor: ").Bold();
                        text.Span(sale.SellerName);
                    });
                });
            });

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(40);
                    columns.RelativeColumn();
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    foreach (var label in new[] { "Cant.", "Unid.", "Descripción", "P. unit.", "Desc.", "Importe" })
                    {
                        header.Cell().Background(Accent).PaddingVertical(5).PaddingHorizontal(4)
                            .Text(label).FontColor(Colors.White).Bold();
                    }
                });

                foreach (var item in sale.Items)
                {
                    table.Cell().Element(Cell).AlignRight().Text(item.Quantity.ToString("0.###", Peru));
                    table.Cell().Element(Cell).Text(item.UnitCode);
                    table.Cell().Element(Cell).Text($"{item.Sku} · {item.ProductName}");
                    table.Cell().Element(Cell).AlignRight().Text(Money(item.UnitPrice));
                    table.Cell().Element(Cell).AlignRight().Text(item.Discount == 0 ? "-" : Money(item.Discount));
                    table.Cell().Element(Cell).AlignRight().Text(Money(item.LineTotal));
                }
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(payments =>
                {
                    payments.Item().Text("Pagos").Bold();
                    foreach (var payment in sale.Payments)
                    {
                        var reference = string.IsNullOrWhiteSpace(payment.Reference) ? string.Empty : $" ({payment.Reference})";
                        payments.Item().Text($"{payment.MethodName}{reference}: {Money(payment.Amount)}").FontColor(Muted);
                    }

                    if (!string.IsNullOrWhiteSpace(sale.Notes))
                    {
                        payments.Item().PaddingTop(6).Text($"Notas: {sale.Notes}").FontColor(Muted);
                    }

                    if (sale.Status == "CANCELLED")
                    {
                        payments.Item().PaddingTop(6).Text($"Anulado: {sale.CancelReason}").FontColor("#DC2626").Bold();
                    }
                });

                row.ConstantItem(210).Column(totals =>
                {
                    TotalLine(totals, "Op. gravada", sale.Subtotal, false);
                    if (sale.DiscountTotal > 0)
                    {
                        TotalLine(totals, "Descuentos", sale.DiscountTotal, false);
                    }

                    TotalLine(totals, "IGV (18%)", sale.TaxAmount, false);
                    TotalLine(totals, "Importe total", sale.Total, true);
                });
            });
        });
    }

    private static void TotalLine(ColumnDescriptor column, string label, decimal amount, bool highlight)
    {
        column.Item().BorderBottom(highlight ? 0 : 1).BorderColor(Border).PaddingVertical(3).Row(row =>
        {
            var labelText = row.RelativeItem().Text(label);
            var amountText = row.ConstantItem(90).AlignRight().Text(Money(amount));

            if (highlight)
            {
                labelText.Bold().FontSize(11);
                amountText.Bold().FontSize(11);
            }
        });
    }

    private static IContainer Cell(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Border).PaddingVertical(4).PaddingHorizontal(4);
    }

    private static string Money(decimal amount)
    {
        return "S/ " + amount.ToString("N2", Peru);
    }
}
