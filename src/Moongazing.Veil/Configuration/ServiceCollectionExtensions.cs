using Microsoft.Extensions.DependencyInjection;
using Moongazing.Veil.Detection;
using Moongazing.Veil.ObjectMasking;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Configuration;

/// <summary>
/// Extension methods for registering Veil services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Veil masking services to the specified <see cref="IServiceCollection"/>.
    /// Configures the static <see cref="Veil"/> entry point and registers
    /// <see cref="VeilPatternRegistry"/>, <see cref="SensitiveDataDetector"/>,
    /// <see cref="ObjectMasker"/>, and <see cref="PropertyMaskResolver"/> as singletons.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">An optional delegate to configure <see cref="VeilOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVeil(this IServiceCollection services, Action<VeilOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new VeilOptions();
        configure?.Invoke(options);

        // Configure the static entry point
        Veil.Configure(options);

        // Register the pattern registry
        var registry = VeilPatternRegistry.Default;

        // Register custom patterns from options
        foreach (var kvp in options.CustomPatterns)
        {
            registry.RegisterCustom(kvp.Key, kvp.Value);
        }

        // Register locale-specific patterns
        foreach (var locale in options.Locales)
        {
            Locales.LocaleRegistrar.RegisterLocale(locale, registry);
        }

        var resolver = new PropertyMaskResolver(options.Conventions, options.EnableAutoDetection);
        var masker = new ObjectMasker(registry, resolver, options.DefaultMaskChar);
        var detector = new SensitiveDataDetector(registry);

        services.AddSingleton(options);
        services.AddSingleton(registry);
        services.AddSingleton(resolver);
        services.AddSingleton(masker);
        services.AddSingleton(detector);

        return services;
    }
}
