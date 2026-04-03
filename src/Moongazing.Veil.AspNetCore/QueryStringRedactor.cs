using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Moongazing.Veil.AspNetCore;

/// <summary>
/// Redacts sensitive query string parameters by name.
/// </summary>
internal static class QueryStringRedactor
{
    private const string RedactedValue = "***";

    /// <summary>
    /// Parses the query string and replaces values of redacted parameters with a masked placeholder.
    /// </summary>
    /// <param name="queryString">The original query string.</param>
    /// <param name="redactedParams">The set of parameter names to redact (case-insensitive).</param>
    /// <returns>The sanitized query string including the leading '?', or an empty string if no parameters exist.</returns>
    public static string Redact(QueryString queryString, IReadOnlySet<string> redactedParams)
    {
        if (!queryString.HasValue || redactedParams.Count == 0)
        {
            return queryString.ToString();
        }

        var parsed = QueryHelpers.ParseQuery(queryString.Value);
        if (parsed.Count == 0)
        {
            return queryString.ToString();
        }

        var sanitized = new Dictionary<string, string?>(parsed.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in parsed)
        {
            sanitized[kvp.Key] = redactedParams.Contains(kvp.Key)
                ? RedactedValue
                : kvp.Value.ToString();
        }

        return QueryString.Create(sanitized!).ToString();
    }
}
