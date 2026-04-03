using System.Text;
using System.Text.RegularExpressions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Locales;

/// <summary>
/// Registers EU-wide GDPR-related sensitive data patterns.
/// Includes generic EU VAT number and passport-like patterns.
/// </summary>
public static partial class EuLocale
{
    // EU VAT number: 2-letter country code + 2-15 alphanumeric characters
    [GeneratedRegex(@"\b[A-Z]{2}\d{2,15}\b", RegexOptions.Compiled)]
    private static partial Regex EuVatRegex();

    // Generic passport-like pattern: 1-2 letters + 6-9 digits (common EU format)
    [GeneratedRegex(@"\b[A-Z]{1,2}\d{6,9}\b", RegexOptions.Compiled)]
    private static partial Regex PassportRegex();

    /// <summary>
    /// Registers EU-specific GDPR patterns into the given registry.
    /// </summary>
    /// <param name="registry">The pattern registry to populate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registry"/> is <see langword="null"/>.</exception>
    public static void Register(VeilPatternRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        registry.RegisterCustom("EU_VatNumber", new VeilPatternDefinition(
            EuVatRegex(),
            MaskEuVat,
            "EU VAT identification number"));

        registry.RegisterCustom("EU_Passport", new VeilPatternDefinition(
            PassportRegex(),
            MaskPassport,
            "EU-style passport number (letters + digits)"));
    }

    private static string MaskEuVat(string value, char maskChar)
    {
        if (value.Length < 5)
        {
            return new string(maskChar, value.Length);
        }

        // Keep country code (first 2) and last 2
        var sb = new StringBuilder(value.Length);
        sb.Append(value.AsSpan(0, 2));
        sb.Append(maskChar, value.Length - 4);
        sb.Append(value.AsSpan(value.Length - 2));
        return sb.ToString();
    }

    private static string MaskPassport(string value, char maskChar)
    {
        if (value.Length < 4)
        {
            return new string(maskChar, value.Length);
        }

        // Keep first 2 and last 2
        var sb = new StringBuilder(value.Length);
        sb.Append(value.AsSpan(0, 2));
        sb.Append(maskChar, value.Length - 4);
        sb.Append(value.AsSpan(value.Length - 2));
        return sb.ToString();
    }
}
