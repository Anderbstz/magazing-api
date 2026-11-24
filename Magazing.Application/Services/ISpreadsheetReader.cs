namespace Magazing.Application.Services;

public interface ISpreadsheetReader
{
    IReadOnlyList<SpreadsheetRow> Read(Stream content, string fileName);
}

public sealed record SpreadsheetRow(int Number, IReadOnlyDictionary<string, string> Cells);
