using System.Reflection;
using Moongazing.Veil.ObjectMasking;
using Moongazing.Veil.Patterns;
using Serilog.Core;
using Serilog.Events;

namespace Moongazing.Veil.Serilog;

/// <summary>
/// A Serilog destructuring policy that masks properties decorated with <see cref="VeiledAttribute"/>
/// when objects are destructured during log event creation.
/// </summary>
public sealed class VeilDestructuringPolicy : IDestructuringPolicy
{
    /// <summary>
    /// Attempts to destructure the given value, masking any properties marked with <see cref="VeiledAttribute"/>.
    /// </summary>
    /// <param name="value">The value to destructure.</param>
    /// <param name="propertyValueFactory">The factory for creating log event property values.</param>
    /// <param name="result">The resulting destructured log event property value.</param>
    /// <returns><see langword="true"/> if the value was destructured; otherwise, <see langword="false"/>.</returns>
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        ArgumentNullException.ThrowIfNull(propertyValueFactory);

        result = null!;

        if (value is null)
        {
            return false;
        }

        var type = value.GetType();

        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type.IsEnum)
        {
            return false;
        }

        var veiledProperties = ObjectMasker.GetVeiledProperties(type);

        if (veiledProperties.Length == 0)
        {
            return false;
        }

        var veiledSet = new HashSet<string>(veiledProperties.Length, StringComparer.Ordinal);
        foreach (var prop in veiledProperties)
        {
            veiledSet.Add(prop.Name);
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var logProperties = new List<LogEventProperty>(properties.Length);

        foreach (var prop in properties)
        {
            if (!prop.CanRead)
            {
                continue;
            }

            var propValue = prop.GetValue(value);

            if (veiledSet.Contains(prop.Name) && propValue is string stringValue)
            {
                var attr = prop.GetCustomAttribute<VeiledAttribute>()!;
                var maskedValue = Veil.Mask(stringValue, attr.Pattern);
                logProperties.Add(new LogEventProperty(prop.Name, new ScalarValue(maskedValue)));
            }
            else
            {
                logProperties.Add(new LogEventProperty(prop.Name, propertyValueFactory.CreatePropertyValue(propValue, destructureObjects: true)));
            }
        }

        result = new StructureValue(logProperties, type.Name);
        return true;
    }
}
