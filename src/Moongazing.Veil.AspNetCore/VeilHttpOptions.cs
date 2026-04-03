using Microsoft.AspNetCore.Http;

namespace Moongazing.Veil.AspNetCore;

/// <summary>
/// Configuration options for Veil HTTP request/response redaction middleware.
/// </summary>
public sealed class VeilHttpOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether request bodies and headers should be logged with redaction applied.
    /// </summary>
    public bool LogRequests { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether response bodies and headers should be logged with redaction applied.
    /// </summary>
    public bool LogResponses { get; set; }

    /// <summary>
    /// Gets or sets the maximum body length in bytes to capture for redaction logging. Defaults to 4096.
    /// </summary>
    public int MaxBodyLength { get; set; } = 4096;

    private readonly HashSet<string> _redactedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "X-Api-Key",
        "Cookie",
        "Set-Cookie",
        "X-Forwarded-For",
        "Proxy-Authorization"
    };

    private readonly HashSet<string> _redactedBodyFields = new();
    private readonly HashSet<string> _redactedQueryParams = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Func<HttpContext, bool>> _redactionPredicates = new();

    /// <summary>
    /// Adds header names to the redaction list. Headers in this list will have their values replaced with "***".
    /// </summary>
    /// <param name="headers">The header names to redact.</param>
    public void RedactHeaders(params string[] headers)
    {
        ArgumentNullException.ThrowIfNull(headers);
        foreach (var header in headers)
        {
            _redactedHeaders.Add(header);
        }
    }

    /// <summary>
    /// Adds JSON paths to the body field redaction list. Fields matching these paths will be masked.
    /// Paths use simple dot notation (e.g., "$.password", "$.creditCard").
    /// </summary>
    /// <param name="jsonPaths">The JSON paths of fields to redact.</param>
    public void RedactBodyFields(params string[] jsonPaths)
    {
        ArgumentNullException.ThrowIfNull(jsonPaths);
        foreach (var path in jsonPaths)
        {
            _redactedBodyFields.Add(path);
        }
    }

    /// <summary>
    /// Adds query parameter names to the redaction list. Matching parameters will have their values masked.
    /// </summary>
    /// <param name="paramNames">The query parameter names to redact.</param>
    public void RedactQueryParams(params string[] paramNames)
    {
        ArgumentNullException.ThrowIfNull(paramNames);
        foreach (var param in paramNames)
        {
            _redactedQueryParams.Add(param);
        }
    }

    /// <summary>
    /// Adds a predicate that determines whether additional redaction should be applied for a given request.
    /// When any predicate returns <see langword="true"/>, full redaction is applied to the request/response.
    /// </summary>
    /// <param name="predicate">A function that evaluates the HTTP context.</param>
    public void RedactWhen(Func<HttpContext, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        _redactionPredicates.Add(predicate);
    }

    /// <summary>
    /// Gets the set of header names to redact.
    /// </summary>
    internal IReadOnlySet<string> RedactedHeaders => _redactedHeaders;

    /// <summary>
    /// Gets the set of JSON paths for body field redaction.
    /// </summary>
    internal IReadOnlySet<string> RedactedBodyFields => _redactedBodyFields;

    /// <summary>
    /// Gets the set of query parameter names to redact.
    /// </summary>
    internal IReadOnlySet<string> RedactedQueryParams => _redactedQueryParams;

    /// <summary>
    /// Gets the list of redaction predicates.
    /// </summary>
    internal IReadOnlyList<Func<HttpContext, bool>> RedactionPredicates => _redactionPredicates;
}
