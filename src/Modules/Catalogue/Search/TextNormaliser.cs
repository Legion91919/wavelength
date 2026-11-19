using System.Globalization;
using System.Text;

namespace Wavelength.Catalogue.Search;

/// <summary>
/// Turns display text into a search key: lower case, accents removed ("Méridien" -> "meridien"),
/// apostrophes dropped, every other non letter/digit treated as a word break.
/// </summary>
internal static class TextNormaliser
{
    // Letters that do not decompose into base letter + accent under Unicode FormD.
    private static readonly Dictionary<char, string> Folds = new()
    {
        ['ø'] = "o",
        ['æ'] = "ae",
        ['œ'] = "oe",
        ['ß'] = "ss",
        ['đ'] = "d",
        ['ł'] = "l",
        ['þ'] = "th",
        ['ð'] = "d",
        ['ı'] = "i",
    };

    public static string Normalise(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var decomposed = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        var pendingSpace = false;
        foreach (var raw in decomposed)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(raw);
            if (category is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.EnclosingMark)
            {
                continue;
            }

            if (raw is '\'' or '’')
            {
                continue;
            }

            var c = char.ToLowerInvariant(raw);
            string? piece = Folds.TryGetValue(c, out var fold) ? fold : char.IsLetterOrDigit(c) ? c.ToString() : null;
            if (piece is null)
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(piece);
        }

        return builder.ToString();
    }

    public static string[] Tokens(string? text) =>
        Normalise(text).Split(' ', StringSplitOptions.RemoveEmptyEntries);
}
