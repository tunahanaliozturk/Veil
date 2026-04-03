namespace Moongazing.Veil.Patterns;

/// <summary>
/// Defines the contract for a sensitive data pattern that can detect and mask values.
/// </summary>
public interface IVeilPattern
{
    /// <summary>
    /// Gets the pattern type identifier.
    /// </summary>
    VeilPattern PatternType { get; }

    /// <summary>
    /// Determines whether the specified input matches this pattern.
    /// </summary>
    /// <param name="input">The input string to test.</param>
    /// <returns><see langword="true"/> if the input matches; otherwise, <see langword="false"/>.</returns>
    bool IsMatch(string input);

    /// <summary>
    /// Masks the specified input according to this pattern's masking strategy.
    /// </summary>
    /// <param name="input">The input string to mask.</param>
    /// <param name="maskChar">The character used for masking. Defaults to <c>'*'</c>.</param>
    /// <returns>The masked string.</returns>
    string Mask(string input, char maskChar = '*');
}
