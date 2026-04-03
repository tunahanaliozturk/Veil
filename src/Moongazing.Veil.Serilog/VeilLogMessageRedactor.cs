using Serilog.Core;
using Serilog.Events;

namespace Moongazing.Veil.Serilog;

/// <summary>
/// A Serilog enricher that scans the rendered log message template properties
/// and redacts any values that contain sensitive data patterns.
/// Unlike <see cref="VeilEnricher"/> which scans top-level scalar properties,
/// this enricher recursively inspects structured and sequence values.
/// </summary>
public sealed class VeilLogMessageRedactor : ILogEventEnricher
{
    /// <summary>
    /// Enriches the log event by recursively scanning all property values for sensitive data and redacting them.
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
            var redacted = RedactValue(property.Value);
            if (!ReferenceEquals(redacted, property.Value))
            {
                propertiesToUpdate.Add(new LogEventProperty(property.Key, redacted));
            }
        }

        foreach (var property in propertiesToUpdate)
        {
            logEvent.AddOrUpdateProperty(property);
        }
    }

    private static LogEventPropertyValue RedactValue(LogEventPropertyValue value)
    {
        switch (value)
        {
            case ScalarValue { Value: string stringValue } when !string.IsNullOrEmpty(stringValue):
            {
                var masked = Veil.Mask(stringValue);
                return string.Equals(masked, stringValue, StringComparison.Ordinal)
                    ? value
                    : new ScalarValue(masked);
            }

            case StructureValue structureValue:
            {
                var changed = false;
                var newProperties = new List<LogEventProperty>(structureValue.Properties.Count);

                foreach (var prop in structureValue.Properties)
                {
                    var redacted = RedactValue(prop.Value);
                    if (!ReferenceEquals(redacted, prop.Value))
                    {
                        changed = true;
                    }
                    newProperties.Add(new LogEventProperty(prop.Name, redacted));
                }

                return changed
                    ? new StructureValue(newProperties, structureValue.TypeTag)
                    : value;
            }

            case SequenceValue sequenceValue:
            {
                var changed = false;
                var newElements = new List<LogEventPropertyValue>(sequenceValue.Elements.Count);

                foreach (var element in sequenceValue.Elements)
                {
                    var redacted = RedactValue(element);
                    if (!ReferenceEquals(redacted, element))
                    {
                        changed = true;
                    }
                    newElements.Add(redacted);
                }

                return changed
                    ? new SequenceValue(newElements)
                    : value;
            }

            case DictionaryValue dictionaryValue:
            {
                var changed = false;
                var newElements = new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>(dictionaryValue.Elements.Count);

                foreach (var element in dictionaryValue.Elements)
                {
                    var redactedValue = RedactValue(element.Value);
                    if (!ReferenceEquals(redactedValue, element.Value))
                    {
                        changed = true;
                    }
                    newElements.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>(element.Key, redactedValue));
                }

                return changed
                    ? new DictionaryValue(newElements)
                    : value;
            }

            default:
                return value;
        }
    }
}
