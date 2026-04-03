using System.Text;
using System.Text.RegularExpressions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Locales;

/// <summary>
/// Registers Turkey-specific sensitive data patterns: TC Kimlik (built-in) and Vergi Numarasi.
/// </summary>
public static partial class TurkeyLocale
{
    [GeneratedRegex(@"\b\d{10}\b", RegexOptions.Compiled)]
    private static partial Regex VergiNoRegex();

    /// <summary>
    /// Registers Turkey-specific patterns into the given registry.
    /// </summary>
    /// <param name="registry">The pattern registry to populate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registry"/> is <see langword="null"/>.</exception>
    public static void Register(VeilPatternRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        // TC Kimlik is already registered as a built-in pattern (TurkishIdPattern).
        // Register Vergi Numarasi (Tax Identification Number — 10 digits)
        registry.RegisterCustom("TR_VergiNo", new VeilPatternDefinition(
            VergiNoRegex(),
            MaskVergiNo,
            "Turkish Tax Identification Number (Vergi Numarasi) — 10 digits"));
    }

    private static string MaskVergiNo(string value, char maskChar)
    {
        if (value.Length < 6)
        {
            return new string(maskChar, value.Length);
        }

        var sb = new StringBuilder(value.Length);
        sb.Append(value.AsSpan(0, 3));
        sb.Append(maskChar, value.Length - 6);
        sb.Append(value.AsSpan(value.Length - 3));
        return sb.ToString();
    }
}
