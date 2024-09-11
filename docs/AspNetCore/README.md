# ASP .Net Core Extensions

- [ASP .Net Core Extensions](#asp-net-core-extensions)
  - [ServiceCollection extensions](#servicecollection-extensions)
  - [CultureUtil](#cultureutil)
  - [Ignore paths Middleware](#ignore-paths-middleware)
- [Validation of Mediator requests](#validation-of-mediator-requests)
- [OpenTelemetry](#opentelemetry)
  - [Mediator tracing](#mediator-tracing)

## ServiceCollection extensions

`services.AddAll<T>(IEnumerable<Assembly>)` registers all implementing classes of T in the given assemblies.

## CultureUtil

The culture util provides a simple way to get the culture from a user based on a list of supported cultures. It supports a "fallback-to-child", meaning if your application supports "de-AT" and the user only supports "de-DE", the util returns "de-AT" as the best common language.

Examples:  

```cs
public class AppSettings
{
    public CultureInfo DefaultCulture { get; set; } = null!;
    public CultureInfo[] SupportedCultures { get; set; } = null!;
}
```

Usage:
```cs
// Supported Cultures: de-AT,en-GB
// Default: de-AT
// Header: de-AT,en-US   Result: de-AT (match)
// Header: fr-FR,en-GB   Result: de-AT (match)
// Header: de-DE,en-US   Result: de-AT (first common language)
// Header: fr-FR,en-US   Result: en-GB (first common language)
// Header: fr-FR         Result: de-AT (no match, default)
CultureUtil.FromAcceptLanguageHeader(HttpContext, appSettings.SupportedCultures, appSettings.DefaultCulture)
```

## Ignore paths Middleware

Returns 404 for all configured paths.

This is useful if you, for example, want to avoid a SPA to handle a path. A typo in an API-Url should result in 404 and should not be handled by the SPA.

Usage:
```cs
//For example right before the SPA
app.UseIgnorePaths("/api", "/swagger", "/hangfire");
```

# Validation of Mediator requests

Mediator requests can be validated with a simple decorator. Internally it uses the same validator that is used by ASP.NET Core for the request validation.  

To enable Mediator request validation, simply add the decorator `RequestValidationDecorator` to `IRequestHandler`:

```cs
// SimpleInjector
container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(RequestValidationDecorator<,>));

// Scrutor
services.Decorate(typeof(IRequestHandler<,>), typeof(RequestValidationDecorator<,>));
```

You can then use the `System.ComponentModel`-attributes and the `IValidatableObject`-interface for validating your models. When model validation fails, a `RequestValidationException` gets thrown containing all the model validation errors. You might want to handle that one in your [Exception filter](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-7.0#exception-filters), if you have one.

Example:
```cs
public class GetSomething : IValidatableObject
{
    [Required]
    public string Search { get; set; }

    [Range(1, 100)]
    public int MaxResults { get; set; } = 10;

    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From >= To)
            yield return new ValidationResult("From must be before To.", new[] { nameof(From), nameof(To) });
    }
}
```

This also works with records.

## OpenTelemetry

### Mediator tracing

To easily trace every request and notification sent by our Mediator-package, simply register the tracing decorators:

```cs
container.RegisterMediatorTracingDecorators();

container.RegisterMediator( /* Mediator configuration */ );
```

Also, when configuring the OpenTelemetry-Tracer, you need to enable the instrumentation:
```
services.AddOpenTelemetry()
        .WithTracing(tracer => tracer.AddMediatorInstrumentation());
```

Note that the order matters. You must register the tracing decorators before mediator.