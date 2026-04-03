using System.Text;
using Moongazing.Veil.Configuration;
using Moongazing.Veil.Detection;
using Moongazing.Veil.Locales;
using Moongazing.Veil.ObjectMasking;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil;

/// <summary>
/// Static entry point for masking and redacting sensitive data.
/// Works without DI registration (zero-config mode). When <see cref="ServiceCollectionExtensions.AddVeil"/>
/// is called, it configures this static instance with the provided options.
/// </summary>
public static class Veil
{
    private static volatile VeilPatternRegistry _registry = VeilPatternRegistry.Default;
    private static volatile SensitiveDataDetector _detector = new(VeilPatternRegistry.Default);
    private static volatile ObjectMasker _masker = new();
    private static volatile char _defaultMaskChar = '*';
    private static readonly object ConfigLock = new();

    /// <summary>
    /// Masks the specified input using the given pattern and mask character.
    /// When <paramref name="pattern"/> is <see cref="VeilPattern.Auto"/>, the pattern is auto-detected.
    /// </summary>
    /// <param name="value">The input string to mask.</param>
    /// <param name="pattern">The pattern to apply for masking.</param>
    /// <param name="maskChar">The character used for masking. Defaults to <c>'*'</c>.</param>
    /// <returns>The masked string, or the original string if no pattern matches.</returns>
    public static string Mask(string value, VeilPattern pattern = VeilPattern.Auto, char maskChar = '*')
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var effectiveMaskChar = maskChar == '*' ? _defaultMaskChar : maskChar;

        if (pattern == VeilPattern.Full)
        {
            return new string(effectiveMaskChar, value.Length);
        }

        if (pattern != VeilPattern.Auto)
        {
            var patternImpl = _registry.GetPattern(pattern);
            if (patternImpl is not null)
            {
                return patternImpl.Mask(value, effectiveMaskChar);
            }

            return value;
        }

        // Auto-detect
        return _detector.DetectAndMask(value, effectiveMaskChar);
    }

    /// <summary>
    /// Scans the text for all known sensitive data patterns and masks every occurrence found.
    /// Unlike <see cref="Mask"/> which treats the entire value as a single sensitive item,
    /// this method finds and masks all sensitive data embedded within a larger text.
    /// </summary>
    /// <param name="text">The text to scan and redact.</param>
    /// <param name="maskChar">The character used for masking. Defaults to <c>'*'</c>.</param>
    /// <returns>The text with all detected sensitive data masked.</returns>
    public static string Redact(string text, char maskChar = '*')
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var effectiveMaskChar = maskChar == '*' ? _defaultMaskChar : maskChar;
        var detections = _detector.Detect(text);

        if (detections.Count == 0)
        {
            return text;
        }

        var sb = new StringBuilder(text.Length);
        var lastIndex = 0;

        foreach (var detection in detections)
        {
            // Append text before this detection
            if (detection.StartIndex > lastIndex)
            {
                sb.Append(text.AsSpan(lastIndex, detection.StartIndex - lastIndex));
            }

            // Mask the detected value
            var pattern = _registry.GetPattern(detection.Pattern);
            if (pattern is not null)
            {
                sb.Append(pattern.Mask(detection.OriginalValue, effectiveMaskChar));
            }
            else
            {
                sb.Append(effectiveMaskChar, detection.Length);
            }

            lastIndex = detection.StartIndex + detection.Length;
        }

        // Append remaining text
        if (lastIndex < text.Length)
        {
            sb.Append(text.AsSpan(lastIndex));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates a masked copy of the specified object. Properties marked with <see cref="VeiledAttribute"/>
    /// or matching convention rules will be masked. The original object is not modified.
    /// </summary>
    /// <typeparam name="T">The type of the object to mask.</typeparam>
    /// <param name="obj">The object whose properties should be masked.</param>
    /// <returns>A new instance of <typeparamref name="T"/> with sensitive properties masked.</returns>
    public static T MaskObject<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);
        return _masker.MaskObject(obj);
    }

    /// <summary>
    /// Configures the static Veil instance from the specified options.
    /// Called internally by <see cref="ServiceCollectionExtensions.AddVeil"/>.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    internal static void Configure(VeilOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        lock (ConfigLock)
        {
            _defaultMaskChar = options.DefaultMaskChar;

            var registry = VeilPatternRegistry.Default;

            // Register custom patterns
            foreach (var kvp in options.CustomPatterns)
            {
                registry.RegisterCustom(kvp.Key, kvp.Value);
            }

            // Register locale patterns
            foreach (var locale in options.Locales)
            {
                LocaleRegistrar.RegisterLocale(locale, registry);
            }

            var resolver = new PropertyMaskResolver(options.Conventions, options.EnableAutoDetection);

            _registry = registry;
            _detector = new SensitiveDataDetector(registry);
            _masker = new ObjectMasker(registry, resolver, options.DefaultMaskChar);
        }
    }
}
