using System.Collections.Concurrent;

namespace Moongazing.Veil.Patterns;

/// <summary>
/// Thread-safe registry that holds all <see cref="IVeilPattern"/> implementations.
/// Built-in patterns are registered by default.
/// </summary>
public sealed class VeilPatternRegistry
{
    private readonly ConcurrentDictionary<VeilPattern, IVeilPattern> _patterns = new();
    private readonly ConcurrentDictionary<string, VeilPatternDefinition> _customPatterns = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Lazy<VeilPatternRegistry> DefaultInstance = new(
        () =>
        {
            var registry = new VeilPatternRegistry();
            registry.RegisterBuiltInPatterns();
            return registry;
        },
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Gets the default shared registry instance with all built-in patterns pre-registered.
    /// </summary>
    public static VeilPatternRegistry Default => DefaultInstance.Value;

    /// <summary>
    /// Initializes a new empty instance of the <see cref="VeilPatternRegistry"/> class.
    /// Call <see cref="RegisterBuiltInPatterns"/> to populate with defaults.
    /// </summary>
    public VeilPatternRegistry()
    {
    }

    /// <summary>
    /// Registers all built-in pattern implementations.
    /// </summary>
    public void RegisterBuiltInPatterns()
    {
        Register(new EmailPattern());
        Register(new PhonePattern());
        Register(new CreditCardPattern());
        Register(new IbanPattern());
        Register(new TurkishIdPattern());
        Register(new TokenPattern());
        Register(new ApiKeyPattern());
        Register(new IpAddressPattern());
        Register(new FullMaskPattern());
    }

    /// <summary>
    /// Registers a pattern implementation, replacing any existing pattern of the same type.
    /// </summary>
    /// <param name="pattern">The pattern to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public void Register(IVeilPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        _patterns[pattern.PatternType] = pattern;
    }

    /// <summary>
    /// Registers a named custom pattern definition.
    /// </summary>
    /// <param name="name">A unique name for the custom pattern.</param>
    /// <param name="definition">The custom pattern definition.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is <see langword="null"/>.</exception>
    public void RegisterCustom(string name, VeilPatternDefinition definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(definition);
        _customPatterns[name] = definition;
    }

    /// <summary>
    /// Gets a registered pattern by its type.
    /// </summary>
    /// <param name="patternType">The pattern type to retrieve.</param>
    /// <returns>The pattern implementation, or <see langword="null"/> if not found.</returns>
    public IVeilPattern? GetPattern(VeilPattern patternType)
    {
        _patterns.TryGetValue(patternType, out var pattern);
        return pattern;
    }

    /// <summary>
    /// Gets a registered custom pattern definition by name.
    /// </summary>
    /// <param name="name">The custom pattern name.</param>
    /// <returns>The pattern definition, or <see langword="null"/> if not found.</returns>
    public VeilPatternDefinition? GetCustomPattern(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _customPatterns.TryGetValue(name, out var definition);
        return definition;
    }

    /// <summary>
    /// Gets all registered built-in patterns.
    /// </summary>
    /// <returns>A read-only collection of all registered patterns.</returns>
    public IReadOnlyCollection<IVeilPattern> GetAllPatterns()
    {
        return _patterns.Values.ToArray();
    }

    /// <summary>
    /// Gets all registered custom pattern definitions.
    /// </summary>
    /// <returns>A read-only dictionary of custom patterns keyed by name.</returns>
    public IReadOnlyDictionary<string, VeilPatternDefinition> GetAllCustomPatterns()
    {
        return _customPatterns.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Attempts to automatically detect which pattern matches the given input.
    /// Evaluates patterns in priority order: ApiKey, Token, Email, CreditCard, Iban, TurkishId, Phone, Ipv4.
    /// </summary>
    /// <param name="input">The input string to test.</param>
    /// <returns>The first matching pattern, or <see langword="null"/> if none match.</returns>
    public IVeilPattern? TryDetect(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        // Priority order: more specific patterns first to avoid false positives
        VeilPattern[] priorityOrder =
        [
            VeilPattern.ApiKey,
            VeilPattern.Token,
            VeilPattern.Email,
            VeilPattern.CreditCard,
            VeilPattern.Iban,
            VeilPattern.TurkishId,
            VeilPattern.Phone,
            VeilPattern.Ipv4
        ];

        foreach (var patternType in priorityOrder)
        {
            if (_patterns.TryGetValue(patternType, out var pattern) && pattern.IsMatch(input))
            {
                return pattern;
            }
        }

        return null;
    }
}
