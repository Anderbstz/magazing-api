using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Magazing.Application.Services;

namespace Magazing.Infrastructure.Adapters.Services;

public class SpreadsheetReader : ISpreadsheetReader
{
    private const int MaxRows = 1000;

    public IReadOnlyList<SpreadsheetRow> Read(Stream content, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var table = extension switch
        {
            ".xlsx" => ReadExcel(content),
            ".csv" => ReadCsv(content),
            _ => throw new InvalidOperationException("Formato no soportado. Sube un archivo .xlsx o .csv.")
        };

        if (table.Count == 0)
        {
            throw new InvalidOperationException("El archivo está vacío.");
        }

        var headers = table[0].Values.Select(NormalizeHeader).ToList();
        var rows = new List<SpreadsheetRow>();

        foreach (var (number, values) in table.Skip(1))
        {
            if (values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var cells = new Dictionary<string, string>();
            for (var column = 0; column < headers.Count; column++)
            {
                if (headers[column].Length > 0 && !cells.ContainsKey(headers[column]))
                {
                    cells[headers[column]] = column < values.Count ? values[column].Trim() : string.Empty;
                }
            }

            rows.Add(new SpreadsheetRow(number, cells));
        }

        if (rows.Count > MaxRows)
        {
            throw new InvalidOperationException($"El archivo tiene {rows.Count} filas. El máximo por importación es {MaxRows}.");
        }

        return rows;
    }

    private static List<(int Number, List<string> Values)> ReadExcel(Stream content)
    {
        try
        {
            using var workbook = new XLWorkbook(content);
            var worksheet = workbook.Worksheets.First();
            var used = worksheet.RangeUsed();
            if (used is null)
            {
                return [];
            }

            var lastColumn = used.LastColumn().ColumnNumber();
            return worksheet.RowsUsed()
                .Select(row => (row.RowNumber(), Enumerable.Range(1, lastColumn).Select(column => row.Cell(column).GetFormattedString()).ToList()))
                .ToList();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("No se pudo leer el Excel. Verifica que sea un archivo .xlsx válido.");
        }
    }

    private static List<(int Number, List<string> Values)> ReadCsv(Stream content)
    {
        using var reader = new StreamReader(content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = reader.ReadToEnd().Replace("\r\n", "\n");
        var firstLine = text.Split('\n', 2)[0];
        var delimiter = firstLine.Count(c => c == ';') > firstLine.Count(c => c == ',') ? ';' : ',';

        var rows = new List<(int Number, List<string> Values)>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        var line = 1;
        var rowStart = 1;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (quoted)
            {
                if (c != '"')
                {
                    line += c == '\n' ? 1 : 0;
                    field.Append(c);
                }
                else if (i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    quoted = false;
                }

                continue;
            }

            if (c == '"')
            {
                quoted = true;
            }
            else if (c == delimiter)
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if (c == '\n')
            {
                line++;
                row.Add(field.ToString());
                field.Clear();
                rows.Add((rowStart, row));
                row = [];
                rowStart = line;
            }
            else
            {
                field.Append(c);
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add((rowStart, row));
        }

        return rows;
    }

    private static string NormalizeHeader(string header)
    {
        var builder = new StringBuilder();
        foreach (var c in header.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.IsLetterOrDigit(c) ? c : '_');
            }
        }

        return Regex.Replace(builder.ToString(), "_+", "_").Trim('_');
    }
}
