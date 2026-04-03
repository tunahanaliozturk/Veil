using System.Text;
using System.Text.RegularExpressions;

namespace Moongazing.Veil.Patterns;

/// <summary>
/// Detects and masks international phone numbers.
/// Masking example: "+905551234567" becomes "+90555***4567".
/// Keeps country code + first 3 digits visible, masks middle, keeps last 4 digits.
/// </summary>
public sealed partial class PhonePattern : IVeilPattern
{
    /// <inheritdoc />
    public VeilPattern PatternType => VeilPattern.Phone;

    [GeneratedRegex(@"\+?\d[\d\s\-\(\)]{8,18}\d", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    /// <inheritdoc />
    public bool IsMatch(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return PhoneRegex().IsMatch(input);
    }

    /// <inheritdoc />
    public string Mask(string input, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(input);

        // Extract only digits and remember the prefix
        var digitsOnly = new StringBuilder(input.Length);
        var hasPlus = input.Length > 0 && input[0] == '+';

        for (var i = 0; i < input.Length; i++)
        {
            if (char.IsDigit(input[i]))
            {
                digitsOnly.Append(input[i]);
            }
        }

        var digits = digitsOnly.ToString();
        if (digits.Length < 7)
        {
            return new string(maskChar, input.Length);
        }

        // Determine visible portions:
        // For numbers with +, keep country code (first 2-3 digits) + next 3 + last 4
        // We'll show first 5 digits and last 4 digits of the digit-only portion
        var showFront = Math.Min(5, digits.Length - 4);
        var showBack = 4;
        var maskCount = digits.Length - showFront - showBack;

        if (maskCount < 0)
        {
            return input;
        }

        var sb = new StringBuilder(input.Length);
        if (hasPlus)
        {
            sb.Append('+');
        }

        sb.Append(digits.AsSpan(0, showFront));
        sb.Append(maskChar, maskCount);
        sb.Append(digits.AsSpan(digits.Length - showBack));

        return sb.ToString();
    }

    /// <summary>
    /// Gets the compiled regex used for phone number detection.
    /// </summary>
    /// <returns>The phone regex instance.</returns>
    public static Regex GetRegex() => PhoneRegex();
}
