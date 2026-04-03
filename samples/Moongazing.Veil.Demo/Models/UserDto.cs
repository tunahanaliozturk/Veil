using Moongazing.Veil;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Demo.Models;

/// <summary>
/// Sample DTO that demonstrates the <see cref="VeiledAttribute"/> on various property types.
/// </summary>
public class UserDto
{
    public string Name { get; set; } = string.Empty;

    [Veiled]
    public string Email { get; set; } = string.Empty;

    [Veiled(VeilPattern.CreditCard)]
    public string CardNumber { get; set; } = string.Empty;

    [Veiled(VeilPattern.Phone)]
    public string Phone { get; set; } = string.Empty;

    [Veiled(Show = 0)]
    public string Password { get; set; } = string.Empty;

    [Veiled(VeilPattern.Iban)]
    public string Iban { get; set; } = string.Empty;

    [Veiled(VeilPattern.TurkishId)]
    public string TcKimlik { get; set; } = string.Empty;
}
