using Serilog.Core;
using Serilog.Events;

namespace Moongazing.Veil.Serilog;

/// <summary>
/// A Serilog enricher that scans log event properties for sensitive data
/// and replaces detected values with masked equivalents.
/// </summary>
public sealed class VeilEnricher : ILogEventEnricher
{
    /// <summary>
    /// Enriches the log event by scanning scalar string property values for sensitive data and masking them.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">The factory for creating log event properties.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);

        var propertiesToUpdate = new List<LogEventProperty>();

        foreach (var property in logEvent.Properties)
        {
            if (property.Value is ScalarValue { Value: string stringValue } && !string.IsNullOrEmpty(stringValue))
            {
                var masked = Veil.Mask(stringValue);
                if (!string.Equals(masked, stringValue, StringComparison.Ordinal))
                {
                    propertiesToUpdate.Add(new LogEventProperty(property.Key, new ScalarValue(masked)));
                }
            }
        }

        foreach (var property in propertiesToUpdate)
        {
            logEvent.AddOrUpdateProperty(property);
        }
    }
}
