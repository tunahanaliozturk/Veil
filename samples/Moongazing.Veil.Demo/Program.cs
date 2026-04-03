using System.Globalization;
using Moongazing.Veil;
using Moongazing.Veil.AspNetCore;
using Moongazing.Veil.Configuration;
using Moongazing.Veil.Demo.Models;
using Moongazing.Veil.Extensions;
using Moongazing.Veil.Locales;
using Moongazing.Veil.Patterns;
using Moongazing.Veil.Serilog;
using Serilog;

// ---------------------------------------------------------------------------
// Serilog bootstrap with Veil integration
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .Destructure.WithVeil()
    .Enrich.WithVeilRedaction()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog as the logging provider
    builder.Host.UseSerilog();

    // ------------------------------------------------------------------
    // Register Veil core services with conventions and locale support
    // ------------------------------------------------------------------
    builder.Services.AddVeil(options =>
    {
        options.DefaultMaskChar = '*';

        options.Convention(conv =>
        {
            conv.WhenPropertyName(n => n.Contains("Email", StringComparison.OrdinalIgnoreCase))
                .UsePattern(VeilPattern.Email);

            conv.WhenPropertyName(n => n.Contains("Phone") || n.Contains("Gsm"))
                .UsePattern(VeilPattern.Phone);

            conv.WhenPropertyName(n => n.Contains("Password") || n.Contains("Secret"))
                .UsePattern(VeilPattern.Full);
        });

        options.AddLocale(VeilLocale.Turkey);
    });

    // ------------------------------------------------------------------
    // Register Veil ASP.NET Core middleware services
    // ------------------------------------------------------------------
    builder.Services.AddVeilAspNetCore(http =>
    {
        http.LogRequests = true;
        http.LogResponses = true;
        http.RedactHeaders("X-Custom-Secret");
        http.RedactQueryParams("token", "apiKey");
    });

    // Swagger / OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // ------------------------------------------------------------------
    // Middleware pipeline
    // ------------------------------------------------------------------
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseVeilRedaction();

    // ------------------------------------------------------------------
    // Demo endpoints
    // ------------------------------------------------------------------

    var demo = app.MapGroup("/api/demo").WithTags("Veil Demo");

    // GET /api/demo/mask?value=john@test.com
    demo.MapGet("/mask", (string value) =>
    {
        var masked = Veil.Mask(value);
        return Results.Ok(new { original = value, masked });
    })
    .WithName("MaskValue")
    .WithDescription("Masks a single value using auto-detected pattern.");

    // GET /api/demo/redact?text=...
    demo.MapGet("/redact", (string text) =>
    {
        var redacted = Veil.Redact(text);
        return Results.Ok(new { original = text, redacted });
    })
    .WithName("RedactText")
    .WithDescription("Scans free text for sensitive data and redacts all occurrences.");

    // POST /api/demo/mask-object
    demo.MapPost("/mask-object", (UserDto dto) =>
    {
        var masked = Veil.MaskObject(dto);

        Log.Information("Masked user object: {@User}", masked);

        return Results.Ok(masked);
    })
    .WithName("MaskObject")
    .WithDescription("Accepts a UserDto and returns a masked copy using [Veiled] attributes.");

    // GET /api/demo/patterns
    demo.MapGet("/patterns", () =>
    {
        var examples = new List<PatternExample>
        {
            new()
            {
                Pattern = nameof(VeilPattern.Email),
                Input   = "john.doe@example.com",
                Masked  = Veil.Mask("john.doe@example.com", VeilPattern.Email)
            },
            new()
            {
                Pattern = nameof(VeilPattern.Phone),
                Input   = "+905551234567",
                Masked  = Veil.Mask("+905551234567", VeilPattern.Phone)
            },
            new()
            {
                Pattern = nameof(VeilPattern.CreditCard),
                Input   = "5425123456789012",
                Masked  = Veil.Mask("5425123456789012", VeilPattern.CreditCard)
            },
            new()
            {
                Pattern = nameof(VeilPattern.Iban),
                Input   = "TR330006100519786457841326",
                Masked  = Veil.Mask("TR330006100519786457841326", VeilPattern.Iban)
            },
            new()
            {
                Pattern = nameof(VeilPattern.TurkishId),
                Input   = "12345678901",
                Masked  = Veil.Mask("12345678901", VeilPattern.TurkishId)
            },
            new()
            {
                Pattern = nameof(VeilPattern.Token),
                Input   = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.abc.xyz",
                Masked  = Veil.Mask("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.abc.xyz", VeilPattern.Token)
            },
            new()
            {
                Pattern = nameof(VeilPattern.Ipv4),
                Input   = "192.168.1.100",
                Masked  = Veil.Mask("192.168.1.100", VeilPattern.Ipv4)
            },
            new()
            {
                Pattern = nameof(VeilPattern.Full),
                Input   = "TopSecretPassword123",
                Masked  = Veil.Mask("TopSecretPassword123", VeilPattern.Full)
            }
        };

        return Results.Ok(examples);
    })
    .WithName("ListPatterns")
    .WithDescription("Lists all built-in VeilPattern values with example inputs and masked outputs.");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
