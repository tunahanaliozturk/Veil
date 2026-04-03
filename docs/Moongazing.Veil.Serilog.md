<p align="center">
  <img src="https://raw.githubusercontent.com/tunahanaliozturk/Moongazing.Veil/main/logo.png" alt="Moongazing.Veil" width="128" />
</p>

<h1 align="center">Moongazing.Veil.Serilog</h1>

<p align="center">
  <strong>Automatic PII Redaction for Serilog</strong><br />
  <em>Powered by <a href="https://www.nuget.org/packages/Moongazing.Veil">Moongazing.Veil</a></em>
</p>

---

## Overview

Moongazing.Veil.Serilog integrates the Veil masking engine into your Serilog pipeline. It provides a destructuring policy that masks `[Veiled]`-decorated properties during object destructuring, and an enricher that redacts sensitive data in log event properties.

No changes to your existing log statements required. Add the integration once, and every log becomes PII-safe.

---

## Installation

```bash
dotnet add package Moongazing.Veil.Serilog
```

This package depends on `Moongazing.Veil` (installed automatically) and `Serilog`.

---

## Quick Start

```csharp
using Moongazing.Veil.Serilog;

Log.Logger = new LoggerConfiguration()
    .Destructure.WithVeil()         // Mask [Veiled] properties during object destructuring
    .Enrich.WithVeilRedaction()     // Scan string properties for sensitive data
    .WriteTo.Console()
    .CreateLogger();
```

---

## Features

### Destructuring Policy

When Serilog destructures an object (e.g., `Log.Information("User {@User}", user)`), the `VeilDestructuringPolicy` inspects each property for the `[Veiled]` attribute and masks its value.

```csharp
public class UserDto
{
    public string Name { get; set; }

    [Veiled]
    public string Email { get; set; }

    [Veiled(VeilPattern.CreditCard)]
    public string CardNumber { get; set; }
}

var user = new UserDto
{
    Name = "John Doe",
    Email = "john.doe@gmail.com",
    CardNumber = "5425123456789012"
};

Log.Information("User {@User}", user);
```

Output:
```
User { Name: "John Doe", Email: "j******e@g****.com", CardNumber: "5425********9012" }
```

### Log Property Enricher

The `VeilEnricher` scans all scalar string properties in a log event and masks any detected sensitive data using `Veil.Mask()`.

```csharp
Log.Information("Login from {Email}", "john.doe@gmail.com");
// Output: Login from j******e@g****.com

Log.Error("Payment failed for card {Card}", "5425123456789012");
// Output: Payment failed for card 5425********9012
```

### Log Message Redactor

The `VeilLogMessageRedactor` (registered as part of `WithVeilRedaction()`) performs deep inspection of all property value types -- scalars, structures, sequences, and dictionaries -- ensuring sensitive data is masked regardless of how it is structured in the log event.

---

## Configuration

### Destructuring Only

If you only want object destructuring (no automatic string scanning):

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.WithVeil()
    .WriteTo.Console()
    .CreateLogger();
```

### Full Redaction

For both destructuring and automatic string property scanning:

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.WithVeil()
    .Enrich.WithVeilRedaction()
    .WriteTo.Console()
    .CreateLogger();
```

### Combined with Core Configuration

```csharp
builder.Services.AddVeil(options =>
{
    options.Convention(conv =>
    {
        conv.WhenPropertyName(n => n.Contains("Email"))
            .UsePattern(VeilPattern.Email);
    });
});

Log.Logger = new LoggerConfiguration()
    .Destructure.WithVeil()
    .Enrich.WithVeilRedaction()
    .WriteTo.Console()
    .CreateLogger();
```

---

## How It Works

| Component | What It Does | When It Runs |
|---|---|---|
| `VeilDestructuringPolicy` | Masks `[Veiled]` properties on destructured objects | During `{@Object}` destructuring |
| `VeilEnricher` | Scans top-level scalar string properties for PII | On every log event |
| `VeilLogMessageRedactor` | Deep-scans all property value types (scalar, structure, sequence, dictionary) | On every log event |

All components are stateless and thread-safe. They add negligible overhead to your logging pipeline.

---

## Requirements

- .NET 8.0 or .NET 9.0
- Serilog 4.x

---

## Related Packages

| Package | Description |
|---|---|
| [`Moongazing.Veil`](https://www.nuget.org/packages/Moongazing.Veil) | Core masking library |
| [`Moongazing.Veil.AspNetCore`](https://www.nuget.org/packages/Moongazing.Veil.AspNetCore) | ASP.NET Core HTTP middleware |

---

## License

[MIT](https://github.com/tunahanaliozturk/Moongazing.Veil/blob/main/LICENSE) -- Copyright (c) Tunahan Ali Ozturk. All rights reserved. See LICENSE for details.
