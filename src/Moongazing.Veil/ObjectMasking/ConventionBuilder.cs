using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.ObjectMasking;

/// <summary>
/// Provides a fluent API for defining convention-based property masking rules.
/// </summary>
public sealed class ConventionBuilder
{
    private readonly List<ConventionRule> _rules = [];

    /// <summary>
    /// Defines a rule that applies when a property name matches the given predicate.
    /// </summary>
    /// <param name="predicate">A function that tests property names.</param>
    /// <returns>A <see cref="ConventionRule"/> for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public ConventionRule WhenPropertyName(Func<string, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var rule = new ConventionRule(predicate);
        _rules.Add(rule);
        return rule;
    }

    /// <summary>
    /// Gets all configured convention rules.
    /// </summary>
    /// <returns>A read-only list of all rules.</returns>
    public IReadOnlyList<ConventionRule> GetRules() => _rules;
}

/// <summary>
/// Represents a single convention-based masking rule that maps a property name predicate to a pattern.
/// </summary>
public sealed class ConventionRule
{
    /// <summary>
    /// Gets the predicate that tests whether a property name matches this rule.
    /// </summary>
    public Func<string, bool> Predicate { get; }

    /// <summary>
    /// Gets the masking pattern to apply when this rule matches.
    /// </summary>
    public VeilPattern Pattern { get; private set; } = VeilPattern.Full;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConventionRule"/> class.
    /// </summary>
    /// <param name="predicate">The property name predicate.</param>
    internal ConventionRule(Func<string, bool> predicate)
    {
        Predicate = predicate;
    }

    /// <summary>
    /// Sets the masking pattern to use when this rule matches.
    /// </summary>
    /// <param name="pattern">The pattern to apply.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public ConventionRule UsePattern(VeilPattern pattern)
    {
        Pattern = pattern;
        return this;
    }
}
