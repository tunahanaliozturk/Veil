namespace Moongazing.Veil.Patterns;

/// <summary>
/// Masks the entire input value unconditionally.
/// Masking example: "mysecretpassword" becomes "****************".
/// </summary>
public sealed class FullMaskPattern : IVeilPattern
{
    /// <inheritdoc />
    public VeilPattern PatternType => VeilPattern.Full;

    /// <summary>
    /// Always returns <see langword="true"/> since full masking applies to any input.
    /// </summary>
    /// <param name="input">The input string to test.</param>
    /// <returns>Always <see langword="true"/>.</returns>
    public bool IsMatch(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return true;
    }

    /// <inheritdoc />
    public string Mask(string input, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(input);
        return new string(maskChar, input.Length);
    }
}
