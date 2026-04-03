using System.Text.RegularExpressions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Detection;

/// <summary>
/// Scans text for all occurrences of sensitive data using registered patterns.
/// </summary>
public sealed class SensitiveDataDetector
{
    private readonly VeilPatternRegistry _registry;

    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveDataDetector"/> class
    /// using the specified pattern registry.
    /// </summary>
    /// <param name="registry">The pattern registry to use for detection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registry"/> is <see langword="null"/>.</exception>
    public SensitiveDataDetector(VeilPatternRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveDataDetector"/> class
    /// using the default pattern registry.
    /// </summary>
    public SensitiveDataDetector()
        : this(VeilPatternRegistry.Default)
    {
    }

    /// <summary>
    /// Determines whether the specified input contains sensitive data.
    /// </summary>
    /// <param name="input">The input string to test.</param>
    /// <returns><see langword="true"/> if sensitive data is detected; otherwise, <see langword="false"/>.</returns>
    public bool ContainsSensitiveData(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        foreach (var pattern in _registry.GetAllPatterns())
        {
            if (pattern.PatternType != VeilPattern.Full && pattern.IsMatch(input))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Detects sensitive data in the input and masks it using the first matching pattern.
    /// </summary>
    /// <param name="input">The input string to scan.</param>
    /// <param name="maskChar">The character used for masking.</param>
    /// <returns>The masked string if a pattern matched; otherwise, the original input.</returns>
    public string DetectAndMask(string input, char maskChar = '*')
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var detected = _registry.TryDetect(input);
        return detected is not null ? detected.Mask(input, maskChar) : input;
    }

    /// <summary>
    /// Scans the specified text for all occurrences of sensitive data.
    /// </summary>
    /// <param name="text">The text to scan.</param>
    /// <returns>A read-only list of all detected sensitive data occurrences, ordered by position.</returns>
    public IReadOnlyList<DetectionResult> Detect(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<DetectionResult>();
        }

        var results = new List<DetectionResult>();

        // Collect regex-based matches from each pattern that exposes a regex
        foreach (var pattern in _registry.GetAllPatterns())
        {
            if (pattern.PatternType == VeilPattern.Full)
            {
                continue; // FullMask matches everything — skip for scanning
            }

            var regex = GetRegexFromPattern(pattern);
            if (regex is null)
            {
                continue;
            }

            foreach (Match match in regex.Matches(text))
            {
                results.Add(new DetectionResult(
                    pattern.PatternType,
                    match.Index,
                    match.Length,
                    match.Value));
            }
        }

        // Also scan custom patterns
        foreach (var kvp in _registry.GetAllCustomPatterns())
        {
            foreach (Match match in kvp.Value.Regex.Matches(text))
            {
                results.Add(new DetectionResult(
                    VeilPattern.Custom,
                    match.Index,
                    match.Length,
                    match.Value));
            }
        }

        // Sort by position, then by length descending (longer matches first)
        results.Sort((a, b) =>
        {
            var posComparison = a.StartIndex.CompareTo(b.StartIndex);
            return posComparison != 0 ? posComparison : b.Length.CompareTo(a.Length);
        });

        // Remove overlapping matches: keep the first (longest) at each position
        var filtered = new List<DetectionResult>(results.Count);
        var lastEnd = -1;

        foreach (var result in results)
        {
            if (result.StartIndex >= lastEnd)
            {
                filtered.Add(result);
                lastEnd = result.StartIndex + result.Length;
            }
        }

        return filtered;
    }

    private static Regex? GetRegexFromPattern(IVeilPattern pattern)
    {
        return pattern switch
        {
            EmailPattern => EmailPattern.GetRegex(),
            PhonePattern => PhonePattern.GetRegex(),
            CreditCardPattern => CreditCardPattern.GetRegex(),
            IbanPattern => IbanPattern.GetRegex(),
            TurkishIdPattern => TurkishIdPattern.GetRegex(),
            TokenPattern => TokenPattern.GetRegex(),
            ApiKeyPattern => ApiKeyPattern.GetRegex(),
            IpAddressPattern => IpAddressPattern.GetRegex(),
            _ => null
        };
    }
}
