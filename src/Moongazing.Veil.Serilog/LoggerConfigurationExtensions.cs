using Serilog;
using Serilog.Configuration;

namespace Moongazing.Veil.Serilog;

/// <summary>
/// Extension methods for <see cref="LoggerDestructuringConfiguration"/> to register Veil destructuring policies.
/// </summary>
public static class LoggerConfigurationExtensions
{
    /// <summary>
    /// Adds the Veil destructuring policy that automatically masks properties decorated with
    /// the Veiled attribute when objects are destructured in log events.
    /// </summary>
    /// <param name="configuration">The destructuring configuration.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration WithVeil(this LoggerDestructuringConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return configuration.With<VeilDestructuringPolicy>();
    }
}

/// <summary>
/// Extension methods for <see cref="LoggerEnrichmentConfiguration"/> to register Veil enrichers.
/// </summary>
public static class LoggerEnrichmentExtensions
{
    /// <summary>
    /// Adds the Veil redaction enricher that scans log event properties for sensitive data
    /// and masks detected values automatically.
    /// </summary>
    /// <param name="configuration">The enrichment configuration.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration WithVeilRedaction(this LoggerEnrichmentConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return configuration.With<VeilLogMessageRedactor>();
    }
}
