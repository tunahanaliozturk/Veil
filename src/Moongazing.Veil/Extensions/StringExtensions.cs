using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Extensions;

/// <summary>
/// Extension methods for masking and redacting sensitive data in strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Masks this string using the specified pattern.
    /// When <paramref name="pattern"/> is <see cref="VeilPattern.Auto"/>, the pattern is auto-detected.
    /// </summary>
    /// <param name="value">The string value to mask.</param>
    /// <param name="pattern">The masking pattern to apply.</param>
    /// <param name="maskChar">The character used for masking.</param>
    /// <returns>The masked string.</returns>
    public static string Veil(this string value, VeilPattern pattern = VeilPattern.Auto, char maskChar = '*')
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return Moongazing.Veil.Veil.Mask(value, pattern, maskChar);
    }

    /// <summary>
    /// Scans the text for all known sensitive data patterns and masks every occurrence found.
    /// </summary>
    /// <param name="text">The text to scan and redact.</param>
    /// <param name="maskChar">The character used for masking.</param>
    /// <returns>The text with all detected sensitive data masked.</returns>
    public static string RedactAll(this string text, char maskChar = '*')
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return Moongazing.Veil.Veil.Redact(text, maskChar);
    }
}
