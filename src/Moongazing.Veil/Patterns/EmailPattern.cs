using System.Text;
using System.Text.RegularExpressions;

namespace Moongazing.Veil.Patterns;

/// <summary>
/// Detects and masks email addresses.
/// Masking example: "john.doe@gmail.com" becomes "j******e@g****.com".
/// </summary>
public sealed partial class EmailPattern : IVeilPattern
{
    /// <inheritdoc />
    public VeilPattern PatternType => VeilPattern.Email;

    [GeneratedRegex(@"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    /// <inheritdoc />
    public bool IsMatch(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return EmailRegex().IsMatch(input);
    }

    /// <inheritdoc />
    public string Mask(string input, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(input);

        var atIndex = input.IndexOf('@');
        if (atIndex < 0)
        {
            return new string(maskChar, input.Length);
        }

        ReadOnlySpan<char> localPart = input.AsSpan(0, atIndex);
        ReadOnlySpan<char> domainPart = input.AsSpan(atIndex + 1);

        var sb = new StringBuilder(input.Length);

        // Local part: first char + mask + last char (if length > 1)
        if (localPart.Length <= 1)
        {
            sb.Append(localPart);
        }
        else
        {
            sb.Append(localPart[0]);
            sb.Append(maskChar, localPart.Length - 2);
            sb.Append(localPart[^1]);
        }

        sb.Append('@');

        // Domain part: find the last dot for TLD
        var lastDotIndex = domainPart.LastIndexOf('.');
        if (lastDotIndex < 0)
        {
            // No TLD separator found — mask entire domain
            sb.Append(maskChar, domainPart.Length);
        }
        else
        {
            ReadOnlySpan<char> domainName = domainPart[..lastDotIndex];
            ReadOnlySpan<char> tld = domainPart[lastDotIndex..]; // includes the dot

            // Domain name: first char + mask
            if (domainName.Length <= 1)
            {
                sb.Append(domainName);
            }
            else
            {
                sb.Append(domainName[0]);
                sb.Append(maskChar, domainName.Length - 1);
            }

            sb.Append(tld);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the compiled regex used for email detection.
    /// </summary>
    /// <returns>The email regex instance.</returns>
    public static Regex GetRegex() => EmailRegex();
}
