using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Moongazing.Veil.AspNetCore;

/// <summary>
/// ASP.NET Core middleware that intercepts HTTP requests and responses,
/// logging sanitized bodies, headers, and query strings with sensitive data redacted.
/// </summary>
internal sealed partial class VeilRedactionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<VeilRedactionMiddleware> _logger;
    private readonly VeilHttpOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="VeilRedactionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The HTTP redaction options.</param>
    public VeilRedactionMiddleware(
        RequestDelegate next,
        ILogger<VeilRedactionMiddleware> logger,
        IOptions<VeilHttpOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Processes the HTTP request and applies redaction logging.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var shouldFullRedact = ShouldFullRedact(context);

        if (_options.LogRequests)
        {
            await LogRequestAsync(context, shouldFullRedact).ConfigureAwait(false);
        }

        if (_options.LogResponses)
        {
            await InvokeWithResponseCaptureAsync(context, shouldFullRedact).ConfigureAwait(false);
        }
        else
        {
            await _next(context).ConfigureAwait(false);
        }
    }

    private async Task LogRequestAsync(HttpContext context, bool fullRedact)
    {
        context.Request.EnableBuffering();

        var sanitizedHeaders = HeaderRedactor.Redact(context.Request.Headers, _options.RedactedHeaders);
        var sanitizedQuery = QueryStringRedactor.Redact(context.Request.QueryString, _options.RedactedQueryParams);

        var body = string.Empty;
        if (context.Request.ContentLength is > 0)
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync().ConfigureAwait(false);
            context.Request.Body.Position = 0;

            body = TruncateAndRedactBody(rawBody, fullRedact);
        }

        var headersSummary = FormatHeaders(sanitizedHeaders);

        LogHttpRequest(
            _logger,
            context.Request.Method,
            context.Request.Path.Value ?? string.Empty,
            sanitizedQuery,
            headersSummary,
            body);
    }

    private async Task InvokeWithResponseCaptureAsync(HttpContext context, bool fullRedact)
    {
        var originalBodyStream = context.Response.Body;

        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context).ConfigureAwait(false);

            responseBodyStream.Position = 0;
            var rawBody = await new StreamReader(responseBodyStream).ReadToEndAsync().ConfigureAwait(false);
            responseBodyStream.Position = 0;

            var sanitizedHeaders = HeaderRedactor.Redact(context.Response.Headers, _options.RedactedHeaders);
            var body = TruncateAndRedactBody(rawBody, fullRedact);

            var headersSummary = FormatHeaders(sanitizedHeaders);

            LogHttpResponse(
                _logger,
                context.Response.StatusCode,
                headersSummary,
                body);

            await responseBodyStream.CopyToAsync(originalBodyStream).ConfigureAwait(false);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private string TruncateAndRedactBody(string rawBody, bool fullRedact)
    {
        if (string.IsNullOrEmpty(rawBody))
        {
            return string.Empty;
        }

        var truncated = rawBody.Length > _options.MaxBodyLength
            ? rawBody[.._options.MaxBodyLength]
            : rawBody;

        if (fullRedact)
        {
            return Veil.Redact(truncated);
        }

        return _options.RedactedBodyFields.Count > 0
            ? BodyRedactor.Redact(truncated, _options.RedactedBodyFields)
            : truncated;
    }

    private bool ShouldFullRedact(HttpContext context)
    {
        for (var i = 0; i < _options.RedactionPredicates.Count; i++)
        {
            if (_options.RedactionPredicates[i](context))
            {
                return true;
            }
        }

        return false;
    }

    private static string FormatHeaders(Dictionary<string, string> headers)
    {
        return string.Join("; ", headers.Select(static h => string.Concat(h.Key, "=", h.Value)));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "HTTP Request {Method} {Path}{Query} | Headers: {Headers} | Body: {Body}")]
    private static partial void LogHttpRequest(ILogger logger, string method, string path, string query, string headers, string body);

    [LoggerMessage(Level = LogLevel.Information, Message = "HTTP Response {StatusCode} | Headers: {Headers} | Body: {Body}")]
    private static partial void LogHttpResponse(ILogger logger, int statusCode, string headers, string body);
}
