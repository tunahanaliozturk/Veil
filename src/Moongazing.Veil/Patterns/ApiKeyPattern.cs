using System.Text;
using System.Text.RegularExpressions;

namespace Moongazing.Veil.Patterns;

/// <summary>
/// Detects and masks API keys with common prefixes (sk-, pk-, key-, api_, etc.).
/// Masking example: "sk-abc123def456ghi789" becomes "sk-abc***789".
/// Keeps the prefix, first 3 characters after prefix, and last 3 characters visible.
/// </summary>
public sealed partial class ApiKeyPattern : IVeilPattern
{
    /// <inheritdoc />
    public VeilPattern PatternType => VeilPattern.ApiKey;

    [GeneratedRegex(@"\b(?:sk|pk|key|api|token|secret)[-_][A-Za-z0-9]{10,}", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex ApiKeyRegex();

    /// <inheritdoc />
    public bool IsMatch(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return ApiKeyRegex().IsMatch(input);
    }

    /// <inheritdoc />
    public string Mask(string input, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(input);

        // Find the prefix separator (- or _)
        var separatorIndex = input.IndexOfAny(['-', '_']);
        if (separatorIndex < 0 || separatorIndex >= input.Length - 1)
        {
            return new string(maskChar, input.Length);
        }

        var prefix = input[..(separatorIndex + 1)]; // e.g. "sk-"
        var body = input[(separatorIndex + 1)..];

        if (body.Length <= 6)
        {
            return prefix + new string(maskChar, body.Length);
        }

        var sb = new StringBuilder(input.Length);
        sb.Append(prefix);
        sb.Append(body.AsSpan(0, 3));
        sb.Append(maskChar, body.Length - 6);
        sb.Append(body.AsSpan(body.Length - 3));

        return sb.ToString();
    }

    /// <summary>
    /// Gets the compiled regex used for API key detection.
    /// </summary>
    /// <returns>The API key regex instance.</returns>
    public static Regex GetRegex() => ApiKeyRegex();
}
