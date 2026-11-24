using System.Globalization;
using System.Text;

namespace Magazing.Application.UseCases.Products;

internal static class ImportText
{
    public static string Key(string? value)
    {
        var builder = new StringBuilder();
        foreach (var c in (value ?? string.Empty).Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    public static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleaned = value.Replace("S/", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(" ", string.Empty);
        if (cleaned.Contains(',') && !cleaned.Contains('.'))
        {
            cleaned = cleaned.Replace(',', '.');
        }
        else
        {
            cleaned = cleaned.Replace(",", string.Empty);
        }

        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
            ? number
            : throw new FormatException($"\"{value}\" no es un número válido.");
    }

    public static bool ParseBool(string? value, bool defaultValue)
    {
        return Key(value) switch
        {
            "" => defaultValue,
            "si" or "s" or "true" or "1" or "x" or "yes" => true,
            "no" or "n" or "false" or "0" => false,
            _ => throw new FormatException($"\"{value}\" no es válido en afecto_igv: usa sí o no.")
        };
    }
}
