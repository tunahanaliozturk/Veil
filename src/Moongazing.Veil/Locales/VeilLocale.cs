namespace Moongazing.Veil.Locales;

/// <summary>
/// Defines supported locale identifiers for country/region-specific masking patterns.
/// </summary>
public enum VeilLocale
{
    /// <summary>
    /// Turkey locale — includes TC Kimlik and Vergi Numarasi patterns.
    /// </summary>
    Turkey,

    /// <summary>
    /// Italy locale — includes Codice Fiscale and Partita IVA patterns.
    /// </summary>
    Italy,

    /// <summary>
    /// EU locale — includes generic GDPR-related patterns.
    /// </summary>
    EU
}
