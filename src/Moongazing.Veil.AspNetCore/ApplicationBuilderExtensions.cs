using Microsoft.AspNetCore.Builder;

namespace Moongazing.Veil.AspNetCore;

/// <summary>
/// Extension methods for <see cref="IApplicationBuilder"/> to register the Veil redaction middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the Veil redaction middleware to the HTTP pipeline.
    /// This middleware intercepts requests and responses to log sanitized bodies, headers, and query strings.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseVeilRedaction(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<VeilRedactionMiddleware>();
    }
}
