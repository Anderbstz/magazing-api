using System.Globalization;
using System.Net;
using System.Text;
using Magazing.Domain.Notifications;

namespace Magazing.Application.Notifications;

internal static class EmailTemplates
{
    private static readonly CultureInfo Peru = CultureInfo.GetCultureInfo("es-PE");

    public static EmailContent Welcome(string fullName, string username)
    {
        var body = $"""
            <p>Hola <strong>{Encode(fullName)}</strong>,</p>
            <p>Se creo tu cuenta en Magazing. Tu usuario es <strong>{Encode(username)}</strong>.</p>
            <p>La contrasena te la entregara el administrador. Te recomendamos cambiarla en tu primer ingreso.</p>
            """;

        return new EmailContent(EmailTemplateCodes.Welcome, "Bienvenido a Magazing", Layout("Bienvenido a Magazing", body));
    }

    public static EmailContent Invoice(InvoiceEmailData data)
    {
        var rows = new StringBuilder();
        foreach (var line in data.Lines)
        {
            rows.Append($"""
                <tr>
                  <td style="padding:6px 8px;border-bottom:1px solid #eee">{Encode(line.ProductName)}</td>
                  <td style="padding:6px 8px;border-bottom:1px solid #eee;text-align:right">{line.Quantity:0.###}</td>
                  <td style="padding:6px 8px;border-bottom:1px solid #eee;text-align:right">{Money(line.UnitPrice)}</td>
                  <td style="padding:6px 8px;border-bottom:1px solid #eee;text-align:right">{Money(line.Discount)}</td>
                  <td style="padding:6px 8px;border-bottom:1px solid #eee;text-align:right">{Money(line.LineTotal)}</td>
                </tr>
                """);
        }

        var body = $"""
            <p>Hola <strong>{Encode(data.CustomerName)}</strong>, gracias por tu compra.</p>
            <p><strong>{Encode(data.DocumentType)} {Encode(data.FullNumber)}</strong><br>
            {Encode(data.CustomerDocumentType)} {Encode(data.CustomerDocumentNumber)}<br>
            Fecha: {data.IssuedAtLima:dd/MM/yyyy HH:mm}</p>
            <table style="width:100%;border-collapse:collapse;font-size:14px">
              <thead>
                <tr style="background:#f4f4f5">
                  <th style="padding:6px 8px;text-align:left">Producto</th>
                  <th style="padding:6px 8px;text-align:right">Cant.</th>
                  <th style="padding:6px 8px;text-align:right">P. unit.</th>
                  <th style="padding:6px 8px;text-align:right">Desc.</th>
                  <th style="padding:6px 8px;text-align:right">Importe</th>
                </tr>
              </thead>
              <tbody>{rows}</tbody>
            </table>
            <p style="text-align:right;font-size:14px">
              Op. gravada: {Money(data.Subtotal)}<br>
              IGV: {Money(data.TaxAmount)}<br>
              <strong>Total: {Money(data.Total)}</strong>
            </p>
            <p style="font-size:12px;color:#71717a">Representacion impresa de comprobante de prueba. No tiene validez tributaria.</p>
            """;

        return new EmailContent(
            EmailTemplateCodes.Invoice,
            $"Tu {data.DocumentType.ToLower()} {data.FullNumber} de Magazing",
            Layout($"{data.DocumentType} {data.FullNumber}", body));
    }

    public static string Money(decimal amount)
    {
        return "S/ " + amount.ToString("N2", Peru);
    }

    public static string Encode(string? value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }

    public static string Layout(string title, string body)
    {
        return $"""
            <!doctype html>
            <html lang="es">
            <body style="margin:0;background:#f4f4f5;font-family:Segoe UI,Arial,sans-serif;color:#18181b">
              <div style="max-width:600px;margin:24px auto;background:#ffffff;border-radius:8px;overflow:hidden">
                <div style="background:#18181b;color:#ffffff;padding:16px 24px;font-size:18px;font-weight:600">Magazing</div>
                <div style="padding:24px">
                  <h1 style="font-size:20px;margin:0 0 16px">{Encode(title)}</h1>
                  {body}
                </div>
              </div>
            </body>
            </html>
            """;
    }
}

internal sealed record InvoiceEmailData(
    string DocumentType,
    string FullNumber,
    string CustomerName,
    string CustomerDocumentType,
    string CustomerDocumentNumber,
    DateTime IssuedAtLima,
    IReadOnlyList<InvoiceEmailLine> Lines,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total);

internal sealed record InvoiceEmailLine(string ProductName, decimal Quantity, decimal UnitPrice, decimal Discount, decimal LineTotal);
