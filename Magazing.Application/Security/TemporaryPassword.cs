using System.Security.Cryptography;

namespace Magazing.Application.Security;

public static class TemporaryPassword
{
    public const int ValidHours = 72;

    public const string ClaimType = "pwd_change";

    private const string Letters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%*";
    private const int Length = 10;

    public static string Generate()
    {
        var chars = new List<char>
        {
            Pick(Letters[..24]),
            Pick(Letters[24..]),
            Pick(Digits),
            Pick(Symbols)
        };

        var pool = Letters + Digits;
        while (chars.Count < Length)
        {
            chars.Add(Pick(pool));
        }

        return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }

    private static char Pick(string source)
    {
        return source[RandomNumberGenerator.GetInt32(source.Length)];
    }
}
