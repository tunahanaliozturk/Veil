namespace Moongazing.Veil.Patterns;

/// <summary>
/// Defines the built-in sensitive data patterns supported by Veil.
/// </summary>
public enum VeilPattern
{
    /// <summary>
    /// Automatically detect the most appropriate pattern for the input.
    /// </summary>
    Auto,

    /// <summary>
    /// Email address pattern (e.g., user@domain.com).
    /// </summary>
    Email,

    /// <summary>
    /// Phone number pattern (international format).
    /// </summary>
    Phone,

    /// <summary>
    /// Credit card number pattern (13-19 digits).
    /// </summary>
    CreditCard,

    /// <summary>
    /// International Bank Account Number pattern.
    /// </summary>
    Iban,

    /// <summary>
    /// Turkish national identity number (11 digits).
    /// </summary>
    TurkishId,

    /// <summary>
    /// Bearer token or JWT pattern.
    /// </summary>
    Token,

    /// <summary>
    /// API key pattern (common prefixed key formats).
    /// </summary>
    ApiKey,

    /// <summary>
    /// IPv4 address pattern.
    /// </summary>
    Ipv4,

    /// <summary>
    /// Masks the entire value completely.
    /// </summary>
    Full,

    /// <summary>
    /// A user-defined custom pattern.
    /// </summary>
    Custom
}
