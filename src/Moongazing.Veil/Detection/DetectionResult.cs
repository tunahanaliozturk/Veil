using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Detection;

/// <summary>
/// Represents a detected occurrence of sensitive data within a string.
/// </summary>
/// <param name="Pattern">The type of sensitive data pattern that was detected.</param>
/// <param name="StartIndex">The zero-based starting index of the match in the source string.</param>
/// <param name="Length">The length of the matched text.</param>
/// <param name="OriginalValue">The original matched text.</param>
public sealed record DetectionResult(VeilPattern Pattern, int StartIndex, int Length, string OriginalValue);
