using System.Text;
using System.Text.RegularExpressions;

namespace Moongazing.Veil.Patterns;

/// <summary>
/// Detects and masks bearer tokens and JWT-like strings.
/// Masking example: "Bearer eyJhbGciOiJIUzI1NiIs..." becomes "Bearer eyJh**********...".
/// Keeps the prefix and first few characters visible.
/// </summary>
public sealed partial class TokenPattern : IVeilPattern
{
    /// <inheritdoc />
    public VeilPattern PatternType => VeilPattern.Token;

    // Matches "Bearer <token>" or standalone JWT-like "eyJ..." strings or long hex/base64 strings
    [GeneratedRegex(@"(?:Bearer\s+)?(?:eyJ[A-Za-z0-9_\-]+\.eyJ[A-Za-z0-9_\-]+\.[A-Za-z0-9_\-]+|[A-Za-z0-9+/=_\-]{32,})", RegexOptions.Compiled)]
    private static partial Regex TokenRegex();

    /// <inheritdoc />
    public bool IsMatch(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return TokenRegex().IsMatch(input);
    }

    /// <inheritdoc />
    public string Mask(string input, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(input);

        ReadOnlySpan<char> span = input.AsSpan();
        var sb = new StringBuilder(input.Length);

        // Preserve "Bearer " prefix if present
        if (span.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            sb.Append("Bearer ");
            span = span[7..];
        }

        if (span.Length <= 8)
        {
            sb.Append(maskChar, span.Length);
        }
        else
        {
            // Show first 4 characters, mask the rest
            sb.Append(span[..4]);
            sb.Append(maskChar, span.Length - 4);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the compiled regex used for token detection.
    /// </summary>
    /// <returns>The token regex instance.</returns>
    public static Regex GetRegex() => TokenRegex();
}
