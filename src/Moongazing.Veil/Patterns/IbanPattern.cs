using System.Text;
using System.Text.RegularExpressions;

namespace Moongazing.Veil.Patterns;

/// <summary>
/// Detects and masks International Bank Account Numbers (IBAN).
/// Masking example: "TR330006100519786457841326" becomes "TR33********************26".
/// Keeps first 4 and last 2 characters visible.
/// </summary>
public sealed partial class IbanPattern : IVeilPattern
{
    /// <inheritdoc />
    public VeilPattern PatternType => VeilPattern.Iban;

    [GeneratedRegex(@"\b[A-Z]{2}\d{2}[\s]?[\dA-Z]{4,30}\b", RegexOptions.Compiled)]
    private static partial Regex IbanRegex();

    /// <inheritdoc />
    public bool IsMatch(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return IbanRegex().IsMatch(input.ToUpperInvariant());
    }

    /// <inheritdoc />
    public string Mask(string input, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(input);

        // Strip spaces for processing
        var clean = input.Replace(" ", "", StringComparison.Ordinal);

        if (clean.Length < 6)
        {
            return new string(maskChar, input.Length);
        }

        // Keep first 4 chars (country + check digits) and last 2
        var sb = new StringBuilder(clean.Length);
        sb.Append(clean.AsSpan(0, 4));
        sb.Append(maskChar, clean.Length - 6);
        sb.Append(clean.AsSpan(clean.Length - 2));

        return sb.ToString();
    }

    /// <summary>
    /// Gets the compiled regex used for IBAN detection.
    /// </summary>
    /// <returns>The IBAN regex instance.</returns>
    public static Regex GetRegex() => IbanRegex();
}
