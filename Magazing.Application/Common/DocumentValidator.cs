using System.Text.RegularExpressions;

namespace Magazing.Application.Common;

public static partial class DocumentValidator
{
    private static readonly string[] RucPrefixes = ["10", "15", "16", "17", "20"];

    public static bool IsValidRuc(string? ruc)
    {
        return ruc is not null && RucRegex().IsMatch(ruc) && RucPrefixes.Contains(ruc[..2]);
    }

    public static bool IsValidDni(string? dni)
    {
        return dni is not null && DniRegex().IsMatch(dni);
    }

    public static bool IsValidEmail(string? email)
    {
        return email is not null && EmailRegex().IsMatch(email);
    }

    [GeneratedRegex("^[0-9]{11}$")]
    private static partial Regex RucRegex();

    [GeneratedRegex("^[0-9]{8}$")]
    private static partial Regex DniRegex();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
