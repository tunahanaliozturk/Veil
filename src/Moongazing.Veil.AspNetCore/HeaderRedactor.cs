using Microsoft.AspNetCore.Http;

namespace Moongazing.Veil.AspNetCore;

/// <summary>
/// Redacts sensitive HTTP headers by replacing their values with a masked placeholder.
/// </summary>
internal static class HeaderRedactor
{
    private const string RedactedValue = "***";

    /// <summary>
    /// Produces a dictionary of header key-value pairs with sensitive headers redacted.
    /// </summary>
    /// <param name="headers">The header dictionary to sanitize.</param>
    /// <param name="redactedHeaders">The set of header names to redact.</param>
    /// <returns>A dictionary with sanitized header values.</returns>
    public static Dictionary<string, string> Redact(IHeaderDictionary headers, IReadOnlySet<string> redactedHeaders)
    {
        var result = new Dictionary<string, string>(headers.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var header in headers)
        {
            result[header.Key] = redactedHeaders.Contains(header.Key)
                ? RedactedValue
                : header.Value.ToString();
        }

        return result;
    }
}
