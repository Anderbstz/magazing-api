using System.Globalization;
using System.Net;
using System.Text;
using Magazing.Domain.Notifications;

namespace Magazing.Application.Notifications;

internal static class EmailTemplates
{
    private static readonly CultureInfo Peru = CultureInfo.GetCultureInfo("es-PE");

    public static EmailContent AccountAccess(string fullName, string username, string temporaryPassword, DateTime expiresAtLima, bool isNewAccount)
    {
        var intro = isNewAccount
            ? "Se creó tu cuenta en Magazing. Estos son tus datos de acceso:"
            : "El administrador generó un nuevo acceso para tu cuenta de Magazing. La contraseña anterior ya no funciona.";

        var body = $"""
            <p>Hola <strong>{Encode(fullName)}</strong>,</p>
            <p>{intro}</p>
            <table style="border-collapse:collapse;font-size:15px;margin:16px 0">
              <tr><td style="padding:6px 12px 6px 0;color:#71717a">Usuario</td><td style="padding:6px 0"><strong>{Encode(username)}</strong></td></tr>
              <tr><td style="padding:6px 12px 6px 0;color:#71717a">Contraseña temporal</td><td style="padding:6px 0;font-family:Consolas,monospace;font-size:16px"><strong>{Encode(temporaryPassword)}</strong></td></tr>
            </table>
            <p>La contraseña temporal vence el <strong>{expiresAtLima:dd/MM/yyyy}</strong> a las <strong>{expiresAtLima:HH:mm}</strong>. Al ingresar, Magazing te pedirá que crees tu propia contraseña.</p>
            <p style="font-size:12px;color:#71717a">Si no esperabas este correo, avisa al administrador de tu empresa.</p>
            """;

        var subject = isNewAccount ? "Tu cuenta de Magazing" : "Nuevo acceso a Magazing";
        var code = isNewAccount ? EmailTemplateCodes.Welcome : EmailTemplateCodes.AccessReset;
        return new EmailContent(code, subject, Layout(isNewAccount ? "Bienvenido a Magazing" : "Nuevo acceso", body));
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
            <p style="font-size:12px;color:#71717a">Representación impresa de comprobante de prueba. No tiene validez tributaria.</p>
            """;

        return new EmailContent(
            EmailTemplateCodes.Invoice,
            $"Tu {data.DocumentType.ToLower()} {data.FullNumber} de Magazing",
            Layout($"{data.DocumentType} {data.FullNumber}", body));
    }

    public static EmailContent StockAlerts(IReadOnlyList<StockAlertEmailLine> alerts)
    {
        var rows = new StringBuilder();
        foreach (var alert in alerts)
        {
            var reason = alert.AlertType == "LOW_STOCK"
                ? $"Stock en o bajo el mínimo ({alert.Threshold:0.##})"
                : $"Demanda predicha {alert.Threshold:0.##} para esta semana";

            rows.Append($"""
                <tr>
                  <td style="padding:6px 8px;border-bottom:1px solid #eee">{Encode(alert.Sku)} - {Encode(alert.ProductName)}</td>
                  <td style="padding:6px 8px;border-bottom:1px solid #eee">{Encode(alert.WarehouseName)}</td>
                  <td style="padding:6px 8px;border-bottom:1px solid #eee;text-align:right">{alert.CurrentStock:0.##}</td>
                  <td style="padding:6px 8px;border-bottom:1px solid #eee">{Encode(reason)}</td>
                  <td style="padding:6px 8px;border-bottom:1px solid #eee;text-align:right">{alert.SuggestedOrderQty:0.##}</td>
                </tr>
                """);
        }

        var body = $"""
            <p>Se detectaron <strong>{alerts.Count}</strong> productos que necesitan reposición.</p>
            <table style="width:100%;border-collapse:collapse;font-size:14px">
              <thead>
                <tr style="background:#f4f4f5">
                  <th style="padding:6px 8px;text-align:left">Producto</th>
                  <th style="padding:6px 8px;text-align:left">Almacén</th>
                  <th style="padding:6px 8px;text-align:right">Stock</th>
                  <th style="padding:6px 8px;text-align:left">Motivo</th>
                  <th style="padding:6px 8px;text-align:right">Sugerido</th>
                </tr>
              </thead>
              <tbody>{rows}</tbody>
            </table>
            <p>Revisa las alertas en Magazing para generar las órdenes de compra.</p>
            """;

        return new EmailContent(
            EmailTemplateCodes.StockAlert,
            $"Magazing: {alerts.Count} alertas de stock",
            Layout("Alertas de stock", body));
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

internal sealed record StockAlertEmailLine(
    string Sku,
    string ProductName,
    string WarehouseName,
    string AlertType,
    decimal CurrentStock,
    decimal Threshold,
    decimal? SuggestedOrderQty);

internal sealed record InvoiceEmailLine(string ProductName, decimal Quantity, decimal UnitPrice, decimal Discount, decimal LineTotal);
