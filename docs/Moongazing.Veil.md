<p align="center">
  <img src="https://raw.githubusercontent.com/tunahanaliozturk/Moongazing.Veil/main/logo.png" alt="Moongazing.Veil" width="128" />
</p>

<h1 align="center">Moongazing.Veil</h1>

<p align="center">
  <strong>Sensitive Data Masking & PII Redaction for .NET</strong><br />
  <em>Sensitive data never leaks. Not in logs, not in responses, not anywhere.</em>
</p>

---

## Overview

Moongazing.Veil is a high-performance .NET library for masking sensitive data in strings, objects, and text blocks. It ships with built-in patterns for the most common PII types and supports custom patterns, convention-based masking, and locale-aware rules.

Designed to help meet **GDPR**, **KVKK**, and **PCI-DSS** requirements with minimal configuration.

---

## Installation

```bash
dotnet add package Moongazing.Veil
```

**Companion packages** (install separately as needed):

| Package | Purpose |
|---|---|
| [`Moongazing.Veil.AspNetCore`](https://www.nuget.org/packages/Moongazing.Veil.AspNetCore) | HTTP request/response redaction middleware |
| [`Moongazing.Veil.Serilog`](https://www.nuget.org/packages/Moongazing.Veil.Serilog) | Serilog destructuring policy and enricher |

---

## Quick Start

Moongazing.Veil works out of the box with zero configuration. No DI registration required for basic use.

### String Masking

```csharp
using Moongazing.Veil;

// Auto-detect and mask
Veil.Mask("john.doe@gmail.com");       // "j******e@g****.com"
Veil.Mask("+905551234567");             // "+90555***4567"
Veil.Mask("5425 1234 5678 9012");      // "5425 **** **** 9012"
Veil.Mask("TR330006100519786457841326");// "TR33 **** **** **** **13 26"
Veil.Mask("12345678901");              // "123*****901"
Veil.Mask("Bearer eyJhbGciOiJ...");    // "Bearer eyJh***..."

// Explicit pattern
Veil.Mask("some-value", VeilPattern.Full);  // "**********"

// Redact all sensitive data in a text block
Veil.Redact("User john@test.com paid with 5425123456789012");
// "User j***@t***.com paid with 5425********9012"
```

### Extension Methods

```csharp
using Moongazing.Veil.Extensions;

"john@test.com".Veil();                           // "j***@t***.com"
"Text with john@test.com in it".RedactAll();       // masked text
UserDto masked = user.VeilProperties();            // new masked copy
```

### Object Masking with `[Veiled]`

```csharp
using Moongazing.Veil;

public class UserDto
{
    public string Name { get; set; }

    [Veiled]
    public string Email { get; set; }

    [Veiled(VeilPattern.CreditCard)]
    public string CardNumber { get; set; }

    [Veiled(VeilPattern.Phone)]
    public string Phone { get; set; }

    [Veiled(Show = 0)]
    public string Password { get; set; }
}

UserDto masked = Veil.MaskObject(user);
// masked.Email      -> "j***@g***.com"
// masked.CardNumber -> "5425 **** **** 9012"
// masked.Password   -> "********"
// Original object is NOT modified
```

### Convention-Based Masking

No attributes needed. Define rules based on property names at startup.

```csharp
builder.Services.AddVeil(options =>
{
    options.Convention(conv =>
    {
        conv.WhenPropertyName(n => n.Contains("Email", StringComparison.OrdinalIgnoreCase))
            .UsePattern(VeilPattern.Email);

        conv.WhenPropertyName(n => n.Contains("Password") || n.Contains("Secret"))
            .UsePattern(VeilPattern.Full);
    });
});
```

### Custom Patterns

```csharp
builder.Services.AddVeil(options =>
{
    options.AddPattern("TaxId", new VeilPatternDefinition
    {
        Regex = @"\b\d{10}\b",
        MaskStrategy = (value, maskChar) =>
            $"{value[..3]}{new string(maskChar, 4)}{value[^3..]}",
        Description = "Tax identification number"
    });
});
```

### Locale Support

```csharp
builder.Services.AddVeil(options =>
{
    options.AddLocale(VeilLocale.Turkey);  // TC Kimlik, Vergi No
    options.AddLocale(VeilLocale.Italy);   // Codice Fiscale, Partita IVA
    options.AddLocale(VeilLocale.EU);      // EU VAT, Passport
});
```

---

## Built-in Patterns

| Pattern | Example Input | Masked Output |
|---|---|---|
| `Email` | `john.doe@gmail.com` | `j******e@g****.com` |
| `Phone` | `+905551234567` | `+90555***4567` |
| `CreditCard` | `5425 1234 5678 9012` | `5425 **** **** 9012` |
| `Iban` | `TR330006100519786457841326` | `TR33 **** **** **** **13 26` |
| `TurkishId` | `12345678901` | `123*****901` |
| `Token` | `Bearer eyJhbGciOiJIUzI1NiIs...` | `Bearer eyJh***...` |
| `ApiKey` | `sk-abc123def456ghi789` | `sk-abc***789` |
| `Ipv4` | `192.168.1.100` | `192.168.*.*` |
| `Full` | `mysecretpassword` | `****************` |

---

## Performance

- **`[GeneratedRegex]`** source generators for all built-in patterns -- near-zero startup cost
- **Aggressive caching** of reflection metadata and pattern match results via `ConcurrentDictionary`
- **`ReadOnlySpan<char>`** in hot paths to minimize heap allocations
- **Immutable operations** -- `MaskObject` returns a new instance, original is never mutated

---

## Configuration Reference

```csharp
builder.Services.AddVeil(options =>
{
    options.DefaultMaskChar = '#';            // Default: '*'
    options.EnableAutoDetection = true;       // Default: true
    options.Convention(conv => { });          // Convention rules
    options.AddPattern("name", definition);   // Custom patterns
    options.AddLocale(VeilLocale.Turkey);     // Locale patterns
});
```

---

## License

[MIT](https://github.com/tunahanaliozturk/Moongazing.Veil/blob/main/LICENSE) -- Copyright (c) Moongazing
