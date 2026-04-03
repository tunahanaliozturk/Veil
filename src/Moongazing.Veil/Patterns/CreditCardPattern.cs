using System.Text;
using System.Text.RegularExpressions;

namespace Moongazing.Veil.Patterns;

/// <summary>
/// Detects and masks credit card numbers (13-19 digits, with optional spaces or dashes).
/// Masking example: "5425 1234 5678 9012" becomes "5425 **** **** 9012".
/// Keeps first 4 and last 4 digits visible.
/// </summary>
public sealed partial class CreditCardPattern : IVeilPattern
{
    /// <inheritdoc />
    public VeilPattern PatternType => VeilPattern.CreditCard;

    [GeneratedRegex(@"\b\d{4}[\s\-]?\d{4}[\s\-]?\d{4}[\s\-]?\d{1,7}\b", RegexOptions.Compiled)]
    private static partial Regex CreditCardRegex();

    /// <inheritdoc />
    public bool IsMatch(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return CreditCardRegex().IsMatch(input);
    }

    /// <inheritdoc />
    public string Mask(string input, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(input);

        // Extract digits only
        var digits = new StringBuilder(20);
        var separators = new List<(int Position, char Separator)>();
        var digitIndex = 0;

        for (var i = 0; i < input.Length; i++)
        {
            if (char.IsDigit(input[i]))
            {
                digits.Append(input[i]);
                digitIndex++;
            }
            else if (input[i] is ' ' or '-')
            {
                separators.Add((digitIndex, input[i]));
            }
        }

        var digitStr = digits.ToString();
        if (digitStr.Length < 8)
        {
            return new string(maskChar, input.Length);
        }

        // Mask: keep first 4, mask middle, keep last 4
        var masked = new StringBuilder(digitStr.Length);
        for (var i = 0; i < digitStr.Length; i++)
        {
            masked.Append(i < 4 || i >= digitStr.Length - 4 ? digitStr[i] : maskChar);
        }

        // Re-insert separators
        var result = new StringBuilder(input.Length);
        digitIndex = 0;
        var sepIdx = 0;

        for (var i = 0; i < masked.Length || sepIdx < separators.Count; i++)
        {
            // Insert any separators that belong at this digit position
            while (sepIdx < separators.Count && separators[sepIdx].Position == digitIndex)
            {
                result.Append(separators[sepIdx].Separator);
                sepIdx++;
            }

            if (digitIndex < masked.Length)
            {
                result.Append(masked[digitIndex]);
                digitIndex++;
            }
        }

        // Trailing separators
        while (sepIdx < separators.Count)
        {
            result.Append(separators[sepIdx].Separator);
            sepIdx++;
        }

        return result.ToString();
    }

    /// <summary>
    /// Gets the compiled regex used for credit card detection.
    /// </summary>
    /// <returns>The credit card regex instance.</returns>
    public static Regex GetRegex() => CreditCardRegex();
}
