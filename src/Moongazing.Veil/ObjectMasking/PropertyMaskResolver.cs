using System.Reflection;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.ObjectMasking;

/// <summary>
/// Resolves which masking pattern to apply to a given property.
/// Priority order: [Veiled] attribute, convention rules, auto-detection.
/// </summary>
public sealed class PropertyMaskResolver
{
    private readonly ConventionBuilder? _conventions;
    private readonly bool _enableAutoDetection;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyMaskResolver"/> class.
    /// </summary>
    /// <param name="conventions">Optional convention builder with name-based rules.</param>
    /// <param name="enableAutoDetection">Whether to fall back to automatic pattern detection.</param>
    public PropertyMaskResolver(ConventionBuilder? conventions = null, bool enableAutoDetection = true)
    {
        _conventions = conventions;
        _enableAutoDetection = enableAutoDetection;
    }

    /// <summary>
    /// Resolves the masking pattern for the specified property.
    /// </summary>
    /// <param name="property">The property to resolve a pattern for.</param>
    /// <param name="value">The current string value of the property.</param>
    /// <returns>The resolved pattern, or <see langword="null"/> if no pattern applies.</returns>
    public VeilPattern? Resolve(PropertyInfo property, string? value)
    {
        ArgumentNullException.ThrowIfNull(property);

        // Priority 1: [Veiled] attribute
        var attribute = property.GetCustomAttribute<VeiledAttribute>();
        if (attribute is not null)
        {
            return attribute.Pattern;
        }

        // Priority 2: Convention rules
        if (_conventions is not null)
        {
            var rules = _conventions.GetRules();
            for (var i = 0; i < rules.Count; i++)
            {
                if (rules[i].Predicate(property.Name))
                {
                    return rules[i].Pattern;
                }
            }
        }

        // Priority 3: Auto-detection (if enabled and value is not null/empty)
        if (_enableAutoDetection && !string.IsNullOrEmpty(value))
        {
            var detected = VeilPatternRegistry.Default.TryDetect(value);
            if (detected is not null)
            {
                return detected.PatternType;
            }
        }

        return null;
    }
}
