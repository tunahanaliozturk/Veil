using System.Text.RegularExpressions;

namespace Moongazing.Veil.Patterns;

/// <summary>
/// Detects and masks IPv4 addresses.
/// Masking example: "192.168.1.100" becomes "192.168.*.*".
/// Keeps the first two octets visible, masks the last two.
/// </summary>
public sealed partial class IpAddressPattern : IVeilPattern
{
    /// <inheritdoc />
    public VeilPattern PatternType => VeilPattern.Ipv4;

    [GeneratedRegex(@"\b(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\.(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\.(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\.(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\b", RegexOptions.Compiled)]
    private static partial Regex Ipv4Regex();

    /// <inheritdoc />
    public bool IsMatch(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Ipv4Regex().IsMatch(input);
    }

    /// <inheritdoc />
    public string Mask(string input, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(input);

        var parts = input.Split('.');
        if (parts.Length != 4)
        {
            return new string(maskChar, input.Length);
        }

        return $"{parts[0]}.{parts[1]}.{maskChar}.{maskChar}";
    }

    /// <summary>
    /// Gets the compiled regex used for IPv4 detection.
    /// </summary>
    /// <returns>The IPv4 regex instance.</returns>
    public static Regex GetRegex() => Ipv4Regex();
}
