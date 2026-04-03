using System.Text;
using System.Text.RegularExpressions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Locales;

/// <summary>
/// Registers Italy-specific sensitive data patterns: Codice Fiscale and Partita IVA.
/// </summary>
public static partial class ItalyLocale
{
    // Codice Fiscale: 16 alphanumeric characters (e.g., RSSMRA80A01H501U)
    [GeneratedRegex(@"\b[A-Z]{6}\d{2}[A-EHLMPRST]\d{2}[A-Z]\d{3}[A-Z]\b", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex CodiceFiscaleRegex();

    // Partita IVA: 11 digits
    [GeneratedRegex(@"\b\d{11}\b", RegexOptions.Compiled)]
    private static partial Regex PartitaIvaRegex();

    /// <summary>
    /// Registers Italy-specific patterns into the given registry.
    /// </summary>
    /// <param name="registry">The pattern registry to populate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registry"/> is <see langword="null"/>.</exception>
    public static void Register(VeilPatternRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        registry.RegisterCustom("IT_CodiceFiscale", new VeilPatternDefinition(
            CodiceFiscaleRegex(),
            MaskCodiceFiscale,
            "Italian Codice Fiscale — 16-character alphanumeric tax code"));

        registry.RegisterCustom("IT_PartitaIVA", new VeilPatternDefinition(
            PartitaIvaRegex(),
            MaskPartitaIva,
            "Italian Partita IVA — 11-digit VAT number"));
    }

    private static string MaskCodiceFiscale(string value, char maskChar)
    {
        if (value.Length < 6)
        {
            return new string(maskChar, value.Length);
        }

        // Keep first 3 and last 3 visible
        var sb = new StringBuilder(value.Length);
        sb.Append(value.AsSpan(0, 3));
        sb.Append(maskChar, value.Length - 6);
        sb.Append(value.AsSpan(value.Length - 3));
        return sb.ToString();
    }

    private static string MaskPartitaIva(string value, char maskChar)
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
