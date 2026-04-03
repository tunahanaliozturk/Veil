<p align="center">
  <img src="https://raw.githubusercontent.com/tunahanaliozturk/Moongazing.Veil/main/logo.png" alt="Moongazing.Veil" width="128" />
</p>

<h1 align="center">Moongazing.Veil.AspNetCore</h1>

<p align="center">
  <strong>HTTP Request/Response PII Redaction Middleware for ASP.NET Core</strong><br />
  <em>Powered by <a href="https://www.nuget.org/packages/Moongazing.Veil">Moongazing.Veil</a></em>
</p>

---

## Overview

Moongazing.Veil.AspNetCore is an ASP.NET Core middleware that automatically redacts sensitive data from HTTP request and response logs. It sanitizes headers, JSON body fields, and query string parameters before they reach your logging infrastructure.

Designed to work seamlessly with the [Moongazing.Veil](https://www.nuget.org/packages/Moongazing.Veil) core library.

---

## Installation

```bash
dotnet add package Moongazing.Veil.AspNetCore
```

This package depends on `Moongazing.Veil` (installed automatically).

---

## Quick Start

```csharp
// Program.cs
builder.Services.AddVeil();
builder.Services.AddVeilAspNetCore();

var app = builder.Build();
app.UseVeilRedaction();
```

That's it. Default configuration redacts `Authorization`, `X-Api-Key`, `Cookie`, and `Set-Cookie` headers out of the box.

---

## Configuration

```csharp
builder.Services.AddVeilAspNetCore(options =>
{
    // Enable request/response body logging with redaction
    options.LogRequests = true;
    options.LogResponses = true;
    options.MaxBodyLength = 4096;

    // Header redaction (defaults already include Authorization, X-Api-Key, Cookie)
    options.RedactHeaders("X-Custom-Secret", "X-Auth-Token");

    // JSON body field redaction using simple path syntax
    options.RedactBodyFields("$.password", "$.creditCard", "$.ssn", "$.token");

    // Query string parameter redaction
    options.RedactQueryParams("api_key", "token", "secret");

    // Conditional full-body redaction for specific endpoints
    options.RedactWhen(context =>
        context.Request.Path.StartsWithSegments("/api/payments"));
});
```

---

## How It Works

The middleware intercepts the HTTP pipeline and performs the following:

1. **Request buffering** -- Enables `EnableBuffering()` so the request body can be read and re-read.
2. **Response wrapping** -- Wraps the response stream to capture the response body for logging.
3. **Header redaction** -- Replaces values of configured headers with `***`.
4. **Body redaction** -- Parses JSON bodies and masks fields matching configured paths using `Veil.Mask()`.
5. **Query string redaction** -- Replaces values of configured query parameters with `***`.
6. **Predicate-based redaction** -- When a request matches a predicate, the entire body is treated as sensitive.

All redaction happens in-memory for logging purposes only. The actual request and response flowing through the pipeline are **not modified**.

---

## Log Output Examples

Before:
```
[INF] POST /api/users | Body: {"email":"john@test.com","password":"secret123"}
[INF] GET /api/data?api_key=sk-abc123def456 | Authorization: Bearer eyJhbG...
```

After:
```
[INF] POST /api/users | Body: {"email":"j***@t***.com","password":"***"}
[INF] GET /api/data?api_key=*** | Authorization: ***
```

---

## Configuration Reference

| Property | Type | Default | Description |
|---|---|---|---|
| `LogRequests` | `bool` | `false` | Log redacted request bodies |
| `LogResponses` | `bool` | `false` | Log redacted response bodies |
| `MaxBodyLength` | `int` | `4096` | Maximum body length to capture (bytes) |
| `RedactHeaders()` | `params string[]` | Auth, Cookie defaults | Headers whose values are replaced with `***` |
| `RedactBodyFields()` | `params string[]` | _(none)_ | JSON paths to redact in request/response bodies |
| `RedactQueryParams()` | `params string[]` | _(none)_ | Query parameters to redact |
| `RedactWhen()` | `Func<HttpContext, bool>` | _(none)_ | Predicate for full-body redaction |

---

## Requirements

- .NET 8.0 or .NET 9.0
- ASP.NET Core

---

## Related Packages

| Package | Description |
|---|---|
| [`Moongazing.Veil`](https://www.nuget.org/packages/Moongazing.Veil) | Core masking library |
| [`Moongazing.Veil.Serilog`](https://www.nuget.org/packages/Moongazing.Veil.Serilog) | Serilog integration |

---

## License

[MIT](https://github.com/tunahanaliozturk/Moongazing.Veil/blob/main/LICENSE) -- Copyright (c) Moongazing
