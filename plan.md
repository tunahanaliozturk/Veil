# Moongazing.Veil

**Tagline:** _"Sensitive data never leaks. Not in logs, not in responses, not anywhere."_

---

## Problem

Sensitive data (email, telefon, kredi kartı, IBAN, TC kimlik, API key, JWT token) her yerde sızıyor:

- Log dosyalarına yazılıyor
- Exception message'larına giriyor
- API response'larında gereksiz yere dönüyor
- Debug output'larında görünüyor
- Webhook payload'larında iletiliyor

GDPR, KVKK, PCI-DSS compliance için bu verilerin maskelenmesi zorunlu. Herkes kendi regex'ini yazıyor, çoğu eksik kalıyor.

---

## Core API Tasarımı

### 1. Standalone Masking — String Seviyesinde

```csharp
// Direkt string masking
string masked = Veil.Mask("john.doe@gmail.com");
// → "j******e@g****.com"

string masked = Veil.Mask("+905551234567");
// → "+90555***4567"

string masked = Veil.Mask("5425 1234 5678 9012");
// → "5425 **** **** 9012"

string masked = Veil.Mask("TR33 0006 1005 1978 6457 8413 26");
// → "TR33 **** **** **** **** **13 26"

string masked = Veil.Mask("12345678901");  // TC Kimlik
// → "123*****901"

// Otomatik tespit — Veil hangi tipte data olduğunu algılar
string masked = Veil.Mask("Bearer eyJhbGciOiJIUzI1NiIs...");
// → "Bearer eyJh***..."

// Bir metin bloğu içindeki tüm sensitive data'yı maskele
string logMessage = "User john@test.com paid with card 5425123456789012";
string safe = Veil.Redact(logMessage);
// → "User j***@t***.com paid with card 5425 **** **** 9012"
```

### 2. Object Masking — Property Seviyesinde

```csharp
// Attribute ile işaretle
public class UserDto
{
    public string Name { get; set; }

    [Veiled]
    public string Email { get; set; }

    [Veiled(VeilPattern.CreditCard)]
    public string CardNumber { get; set; }

    [Veiled(VeilPattern.Phone)]
    public string Phone { get; set; }

    [Veiled(Show = 0)]  // Tamamen gizle
    public string Password { get; set; }

    [Veiled(Show = 4, Position = VeilPosition.Last)]  // Son 4 hane göster
    public string AccountNumber { get; set; }
}

// Object'i maskele
UserDto user = GetUser();
UserDto masked = Veil.MaskObject(user);
// masked.Email → "j***@g***.com"
// masked.CardNumber → "5425 **** **** 9012"
// masked.Password → "********"

// Yeni obje döner, orijinal değişmez (immutable operation)
```

### 3. Convention-Based Masking (Attribute'suz)

```csharp
// Global convention — property adına göre otomatik maskele
services.AddVeil(options =>
{
    options.Convention(conv =>
    {
        // Property adı "Email" içeriyorsa email olarak maskele
        conv.WhenPropertyName(name => name.Contains("Email", StringComparison.OrdinalIgnoreCase))
            .UsePattern(VeilPattern.Email);

        conv.WhenPropertyName(name => name.Contains("Phone") || name.Contains("Gsm"))
            .UsePattern(VeilPattern.Phone);

        conv.WhenPropertyName(name => name.Contains("CardNumber") || name.Contains("CreditCard"))
            .UsePattern(VeilPattern.CreditCard);

        conv.WhenPropertyName(name => name.Contains("Password") || name.Contains("Secret"))
            .UsePattern(VeilPattern.Full);  // Tamamen gizle

        conv.WhenPropertyName(name => name.Contains("Token") || name.Contains("ApiKey"))
            .UsePattern(VeilPattern.Token);

        conv.WhenPropertyName(name => name.Contains("Iban"))
            .UsePattern(VeilPattern.Iban);
    });
});

// Artık [Veiled] attribute koymana gerek yok
// Veil otomatik algılar
UserDto masked = Veil.MaskObject(user); // Email, Phone, Password otomatik maskelenir
```

### 4. HTTP Request/Response Redaction Middleware

```csharp
// Program.cs
services.AddVeil(options =>
{
    // Core masking config
    options.DefaultMaskChar = '*';

    // HTTP logging/redaction
    options.Http(http =>
    {
        http.LogRequests = true;
        http.LogResponses = true;
        http.MaxBodyLength = 4096;  // Body truncation

        // Header redaction
        http.RedactHeaders("Authorization", "X-Api-Key", "Cookie", "Set-Cookie");

        // Body field redaction (JSON path)
        http.RedactBodyFields("$.password", "$.creditCard", "$.ssn", "$.token");

        // Query string redaction
        http.RedactQueryParams("api_key", "token", "secret");

        // Custom redaction rule
        http.RedactWhen(context =>
            context.Request.Path.StartsWithSegments("/api/payments"));
    });
});

app.UseVeilRedaction(); // Middleware ekle

// Artık tüm HTTP logları PII-safe:
// [INF] POST /api/users → 201 | Body: {"email":"j***@g***.com","password":"********"}
// [INF] GET /api/orders?api_key=*** → 200
// [INF] Authorization: Bearer eyJh***...
```

### 5. Serilog Enricher / Destructuring Policy

```csharp
// Serilog entegrasyonu
Log.Logger = new LoggerConfiguration()
    .Destructure.WithVeil()  // Object destructuring'de otomatik maskeleme
    .Enrich.WithVeilRedaction()  // Message template'lerdeki sensitive data'yı maskele
    .CreateLogger();

// Kullanım — normal logla, Veil otomatik maskeler
Log.Information("User {User} logged in from {Email}", user, user.Email);
// Output: User { Name: "John", Email: "j***@g***.com" } logged in from j***@g***.com

Log.Error(ex, "Payment failed for card {CardNumber}", cardNumber);
// Output: Payment failed for card 5425 **** **** 9012
```

### 6. Custom Pattern Tanımlama

```csharp
services.AddVeil(options =>
{
    // Kendi pattern'ını ekle
    options.AddPattern("TurkishId", new VeilPatternDefinition
    {
        Regex = @"\b[1-9]\d{10}\b",
        MaskStrategy = (value) =>
        {
            // İlk 3 + son 3 göster
            return $"{value[..3]}*****{value[^3..]}";
        },
        Description = "Turkish National ID"
    });

    // Ülkeye özel pattern'lar
    options.AddLocale(VeilLocale.Turkey);  // TC Kimlik, Vergi No, IBAN TR
    options.AddLocale(VeilLocale.Italy);   // Codice Fiscale, Partita IVA, IBAN IT
    options.AddLocale(VeilLocale.EU);      // GDPR-aware patterns
});
```

---

## Built-in Patterns (VeilPattern)

|Pattern|Örnek Input|Masked Output|
|---|---|---|
|`Email`|`john.doe@gmail.com`|`j******e@g****.com`|
|`Phone`|`+905551234567`|`+90555***4567`|
|`CreditCard`|`5425123456789012`|`5425 **** **** 9012`|
|`Iban`|`TR330006100519786457841326`|`TR33 **** **** **** **13 26`|
|`TurkishId`|`12345678901`|`123*****901`|
|`Token`|`Bearer eyJhbGci...`|`Bearer eyJh***...`|
|`ApiKey`|`sk-abc123def456ghi789`|`sk-abc***789`|
|`Ipv4`|`192.168.1.100`|`192.168.*.*`|
|`Full`|`mysecretpassword`|`****************`|
|`Custom`|_(user defined)_|_(user defined)_|

---

## Proje Yapısı

```
src/
└── Moongazing.Veil/
    ├── Veil.cs                          # Static entry point: Veil.Mask(), Veil.Redact(), Veil.MaskObject()
    ├── VeiledAttribute.cs               # [Veiled] property attribute
    │
    ├── Patterns/
    │   ├── VeilPattern.cs               # Pattern enum
    │   ├── VeilPatternDefinition.cs     # Custom pattern model
    │   ├── VeilPatternRegistry.cs       # Pattern kayıt ve lookup
    │   ├── EmailPattern.cs              # Email masking logic
    │   ├── PhonePattern.cs              # Phone masking logic
    │   ├── CreditCardPattern.cs         # CC masking logic
    │   ├── IbanPattern.cs               # IBAN masking logic
    │   ├── TokenPattern.cs              # JWT/API key masking
    │   └── IpAddressPattern.cs          # IP masking logic
    │
    ├── Locales/
    │   ├── VeilLocale.cs                # Locale enum
    │   ├── TurkeyLocale.cs              # TC Kimlik, Vergi No
    │   ├── ItalyLocale.cs               # Codice Fiscale, Partita IVA
    │   └── EuLocale.cs                  # GDPR common patterns
    │
    ├── ObjectMasking/
    │   ├── ObjectMasker.cs              # Reflection-based object masking
    │   ├── ConventionBuilder.cs         # Convention-based config
    │   └── PropertyMaskResolver.cs      # Attribute vs convention resolver
    │
    ├── Detection/
    │   ├── SensitiveDataDetector.cs     # Auto-detect sensitive data in strings
    │   └── DetectionResult.cs           # Detection result model
    │
    ├── Http/
    │   ├── VeilRedactionMiddleware.cs   # ASP.NET Core middleware
    │   ├── VeilHttpOptions.cs           # HTTP-specific configuration
    │   ├── HeaderRedactor.cs            # Header sanitization
    │   ├── BodyRedactor.cs              # JSON body field redaction
    │   └── QueryStringRedactor.cs       # Query param redaction
    │
    ├── Logging/
    │   ├── VeilDestructuringPolicy.cs   # Serilog destructuring
    │   ├── VeilEnricher.cs              # Serilog enricher
    │   └── VeilLogMessageRedactor.cs    # Log message scanning
    │
    ├── Configuration/
    │   ├── VeilOptions.cs               # Global options
    │   └── ServiceCollectionExtensions.cs
    │
    └── Extensions/
        ├── StringExtensions.cs          # string.Veil(), string.RedactAll()
        └── ObjectExtensions.cs          # obj.VeilProperties()

tests/
└── Moongazing.Veil.Tests/
    ├── Patterns/
    │   ├── EmailPatternTests.cs
    │   ├── PhonePatternTests.cs
    │   ├── CreditCardPatternTests.cs
    │   ├── IbanPatternTests.cs
    │   └── AutoDetectionTests.cs
    ├── ObjectMasking/
    │   ├── AttributeMaskingTests.cs
    │   ├── ConventionMaskingTests.cs
    │   └── NestedObjectTests.cs
    ├── Http/
    │   ├── MiddlewareTests.cs
    │   ├── HeaderRedactorTests.cs
    │   └── BodyRedactorTests.cs
    └── Logging/
        ├── SerilogIntegrationTests.cs
        └── LogMessageRedactorTests.cs
```

---

## Extension Methods

```csharp
// String extensions
string safe = "john@test.com".Veil();                    // → "j***@t***.com"
string safe = "Some text with john@test.com".RedactAll(); // → "Some text with j***@t***.com"

// Object extensions
UserDto masked = user.VeilProperties();  // Yeni masked kopya döner
```

---

## Büyüme Yol Haritası

|Versiyon|Özellik|
|---|---|
|v1.0|`Veil.Mask()`, `Veil.Redact()`, built-in patterns (Email, Phone, CreditCard, IBAN, Token), `[Veiled]` attribute, `Veil.MaskObject()`|
|v1.1|Convention-based masking (attribute'suz), auto-detection engine|
|v1.2|HTTP middleware (`UseVeilRedaction`), header/body/query redaction|
|v2.0|Serilog integration (destructuring + enricher)|
|v2.1|Locale support (Turkey, Italy, EU)|
|v2.2|Custom pattern API, `VeilPatternDefinition`|
|v3.0|OpenTelemetry span attribute redaction|
|v3.1|Source generator — compile-time object masker (zero reflection)|
|v3.2|Benchmark suite + performance optimization|

---

## Rekabet Analizi

|Paket|Sorun|
|---|---|
|Serilog.Enrichers.Sensitive|Sadece Serilog, object masking yok, pattern seti dar|
|DataMasking.Net|Abandoned, .NET 6+ desteği yok|
|Anonymizer|Sadece fake data üretir, masking yapmaz|
|**Moongazing.Veil**|**All-in-one: string + object + HTTP + logging, locale-aware, convention-based, zero-config modu var**|

---

## NuGet Açıklaması

**Title:** Moongazing.Veil — Sensitive Data Masking & PII-Safe Logging for .NET

**Description:** Automatically mask emails, phone numbers, credit cards, IBANs, tokens, and custom patterns in strings, objects, HTTP logs, and Serilog output. GDPR/KVKK compliant. Zero-config convention mode or fine-grained attribute control. Protect sensitive data everywhere it flows.

**Tags:** `masking`, `redaction`, `pii`, `gdpr`, `kvkk`, `sensitive-data`, `logging`, `security`, `privacy`, `serilog`, `middleware`

---

_Moongazing.Veil — Sensitive data never leaks._