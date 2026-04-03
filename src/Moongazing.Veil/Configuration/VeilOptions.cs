using Moongazing.Veil.Locales;
using Moongazing.Veil.ObjectMasking;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Configuration;

/// <summary>
/// Configuration options for the Veil masking engine.
/// </summary>
public sealed class VeilOptions
{
    private readonly Dictionary<string, VeilPatternDefinition> _customPatterns = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<VeilLocale> _locales = [];

    /// <summary>
    /// Gets or sets the default character used for masking. Defaults to <c>'*'</c>.
    /// </summary>
    public char DefaultMaskChar { get; set; } = '*';

    /// <summary>
    /// Gets or sets a value indicating whether automatic sensitive data detection is enabled.
    /// </summary>
    public bool EnableAutoDetection { get; set; } = true;

    /// <summary>
    /// Gets the convention builder for defining name-based property masking rules.
    /// </summary>
    public ConventionBuilder Conventions { get; } = new();

    /// <summary>
    /// Configures convention-based masking rules using a fluent builder.
    /// </summary>
    /// <param name="configure">A delegate to configure conventions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    public void Convention(Action<ConventionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Conventions);
    }

    /// <summary>
    /// Adds a named custom pattern definition.
    /// </summary>
    /// <param name="name">A unique name for the custom pattern.</param>
    /// <param name="definition">The custom pattern definition.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is <see langword="null"/>.</exception>
    public void AddPattern(string name, VeilPatternDefinition definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(definition);
        _customPatterns[name] = definition;
    }

    /// <summary>
    /// Registers a locale for country/region-specific patterns.
    /// </summary>
    /// <param name="locale">The locale to register.</param>
    public void AddLocale(VeilLocale locale)
    {
        if (!_locales.Contains(locale))
        {
            _locales.Add(locale);
        }
    }

    /// <summary>
    /// Gets the registered custom patterns.
    /// </summary>
    internal IReadOnlyDictionary<string, VeilPatternDefinition> CustomPatterns => _customPatterns;

    /// <summary>
    /// Gets the registered locales.
    /// </summary>
    internal IReadOnlyList<VeilLocale> Locales => _locales;
}
