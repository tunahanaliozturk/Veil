using Microsoft.Extensions.DependencyInjection;

namespace Moongazing.Veil.AspNetCore;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register Veil ASP.NET Core services.
/// </summary>
public static class VeilAspNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Veil HTTP redaction services and configures <see cref="VeilHttpOptions"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An optional delegate to configure <see cref="VeilHttpOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVeilAspNetCore(
        this IServiceCollection services,
        Action<VeilHttpOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<VeilHttpOptions>(_ => { });
        }

        return services;
    }
}
