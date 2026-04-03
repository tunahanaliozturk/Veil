using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Locales;

/// <summary>
/// Internal helper that dispatches locale registration to the correct locale class.
/// </summary>
internal static class LocaleRegistrar
{
    /// <summary>
    /// Registers patterns for the given locale into the registry.
    /// </summary>
    /// <param name="locale">The locale to register.</param>
    /// <param name="registry">The pattern registry to populate.</param>
    internal static void RegisterLocale(VeilLocale locale, VeilPatternRegistry registry)
    {
        switch (locale)
        {
            case VeilLocale.Turkey:
                TurkeyLocale.Register(registry);
                break;
            case VeilLocale.Italy:
                ItalyLocale.Register(registry);
                break;
            case VeilLocale.EU:
                EuLocale.Register(registry);
                break;
        }
    }
}
