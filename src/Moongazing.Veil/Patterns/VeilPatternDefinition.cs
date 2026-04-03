using System.Text.RegularExpressions;

namespace Moongazing.Veil.Patterns;

/// <summary>
/// Defines a custom masking pattern with a regex and a masking strategy.
/// </summary>
public sealed class VeilPatternDefinition
{
    /// <summary>
    /// Gets the compiled regular expression used to detect this pattern.
    /// </summary>
    public Regex Regex { get; }

    /// <summary>
    /// Gets the masking strategy function. Receives the matched value and the mask character,
    /// and returns the masked string.
    /// </summary>
    public Func<string, char, string> MaskStrategy { get; }

    /// <summary>
    /// Gets an optional human-readable description of this pattern.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VeilPatternDefinition"/> class.
    /// </summary>
    /// <param name="regex">The compiled regular expression for detection.</param>
    /// <param name="maskStrategy">The masking strategy function.</param>
    /// <param name="description">An optional description of the pattern.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="regex"/> or <paramref name="maskStrategy"/> is <see langword="null"/>.</exception>
    public VeilPatternDefinition(Regex regex, Func<string, char, string> maskStrategy, string description = "")
    {
        Regex = regex ?? throw new ArgumentNullException(nameof(regex));
        MaskStrategy = maskStrategy ?? throw new ArgumentNullException(nameof(maskStrategy));
        Description = description ?? string.Empty;
    }
}
