using System.Buffers;
using System.Text.Json;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.AspNetCore;

/// <summary>
/// Redacts sensitive fields within JSON request/response bodies.
/// </summary>
internal static class BodyRedactor
{
    private const string NonJsonBody = "[non-json body]";

    /// <summary>
    /// Parses the JSON body and redacts fields matching the configured JSON paths.
    /// Simple path matching supports paths like "$.password" or "$.user.email" (dot-separated property names).
    /// </summary>
    /// <param name="body">The raw JSON body string.</param>
    /// <param name="redactedFields">The set of JSON paths to redact.</param>
    /// <returns>The sanitized JSON string, or a placeholder if the body is not valid JSON.</returns>
    public static string Redact(string body, IReadOnlySet<string> redactedFields)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return body;
        }

        if (redactedFields.Count == 0)
        {
            return body;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var bufferWriter = new ArrayBufferWriter<byte>(body.Length);
            using var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = false });

            RedactElement(writer, document.RootElement, "$", redactedFields);
            writer.Flush();

            return System.Text.Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
        }
        catch (JsonException)
        {
            return NonJsonBody;
        }
    }

    private static void RedactElement(
        Utf8JsonWriter writer,
        JsonElement element,
        string currentPath,
        IReadOnlySet<string> redactedFields)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    var propertyPath = $"{currentPath}.{property.Name}";
                    writer.WritePropertyName(property.Name);

                    if (redactedFields.Contains(propertyPath))
                    {
                        WriteRedactedValue(writer, property.Value);
                    }
                    else
                    {
                        RedactElement(writer, property.Value, propertyPath, redactedFields);
                    }
                }
                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var itemPath = $"{currentPath}[{index}]";
                    RedactElement(writer, item, itemPath, redactedFields);
                    index++;
                }
                writer.WriteEndArray();
                break;

            default:
                element.WriteTo(writer);
                break;
        }
    }

    private static void WriteRedactedValue(Utf8JsonWriter writer, JsonElement original)
    {
        switch (original.ValueKind)
        {
            case JsonValueKind.String:
                var value = original.GetString();
                writer.WriteStringValue(value is not null ? Veil.Mask(value, VeilPattern.Full) : "***");
                break;

            case JsonValueKind.Number:
                writer.WriteStringValue("***");
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                writer.WriteStringValue("***");
                break;

            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;

            default:
                writer.WriteStringValue("***");
                break;
        }
    }
}
