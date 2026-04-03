using System.Text;
using System.Text.RegularExpressions;

namespace Moongazing.Veil.Patterns;

/// <summary>
/// Detects and masks Turkish national identity numbers (TC Kimlik No).
/// Masking example: "12345678901" becomes "123*****901".
/// Keeps first 3 and last 3 digits visible.
/// </summary>
public sealed partial class TurkishIdPattern : IVeilPattern
{
    /// <inheritdoc />
    public VeilPattern PatternType => VeilPattern.TurkishId;

    [GeneratedRegex(@"\b[1-9]\d{10}\b", RegexOptions.Compiled)]
    private static partial Regex TurkishIdRegex();

    /// <inheritdoc />
    public bool IsMatch(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return TurkishIdRegex().IsMatch(input);
    }

    /// <inheritdoc />
    public string Mask(string input, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length < 6)
        {
            return new string(maskChar, input.Length);
        }

        var sb = new StringBuilder(input.Length);
        sb.Append(input.AsSpan(0, 3));
        sb.Append(maskChar, input.Length - 6);
        sb.Append(input.AsSpan(input.Length - 3));

        return sb.ToString();
    }

    /// <summary>
    /// Gets the compiled regex used for Turkish ID detection.
    /// </summary>
    /// <returns>The Turkish ID regex instance.</returns>
    public static Regex GetRegex() => TurkishIdRegex();
}
