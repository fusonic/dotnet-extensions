# ASP .Net Core Extensions

Package: `Fusonic.Extensions.AspNetCore`

- [ASP .Net Core Extensions](#asp-net-core-extensions)
  - [ServiceCollection extensions](#servicecollection-extensions)
  - [CultureUtil](#cultureutil)
  - [Ignore paths Middleware](#ignore-paths-middleware)
  - [Cache headers Middleware](#cache-headers-middleware)
  - [HttpContextUserAccessor](#httpcontextuseraccessor)
- [Validation of Mediator requests](#validation-of-mediator-requests)
- [OpenTelemetry](#opentelemetry)
  - [Mediator tracing](#mediator-tracing)

The package references SimpleInjector and Scrutor with `PrivateAssets="all"`. If you use the SimpleInjector or Scrutor based registrations below, reference `SimpleInjector` or `Scrutor` in your own project.

## ServiceCollection extensions

Namespace: `Fusonic.Extensions.AspNetCore`

`services.AddAll<T>(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Scoped)` registers all non-abstract classes implementing `T` in the given assemblies. `T` must be an interface, otherwise an `ArgumentException` is thrown.

## CultureUtil

Namespace: `Fusonic.Extensions.AspNetCore.Http`

The culture util provides a simple way to get the culture from a user based on a list of supported cultures. It supports a "fallback-to-child", meaning if your application supports "de-AT" and the user only supports "de-DE", the util returns "de-AT" as the best common language.

Example settings:

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
// Header: fr-FR,en-GB   Result: en-GB (match)
// Header: de-DE,en-US   Result: de-AT (first common language)
// Header: fr-FR,en-US   Result: en-GB (first common language)
// Header: fr-FR         Result: de-AT (no match, default)
var culture = CultureUtil.FromAcceptLanguageHeader(HttpContext, appSettings.SupportedCultures, appSettings.DefaultCulture);
```

Weights (`q=`) in the header are respected. An overload takes the parsed header (`IList<StringWithQualityHeaderValue>`) instead of the `HttpContext`. `CultureUtil.GetFirstSupportedCulture(string culture, IEnumerable<CultureInfo> supportedCultures, CultureInfo? defaultCulture = null)` applies the same fallback to a single culture name.

## Ignore paths Middleware

Namespace: `Fusonic.Extensions.AspNetCore.Http`

Returns 404 for all configured paths.

This is useful if you, for example, want to avoid a SPA to handle a path. A typo in an API-Url should result in 404 and should not be handled by the SPA.

Usage:
```cs
// For example right before the SPA
app.UseIgnorePaths("/api", "/swagger", "/hangfire");
```

An overload takes an `IEnumerable<PathString>`.

## Cache headers Middleware

Namespace: `Fusonic.Extensions.AspNetCore.Http` (options in `Fusonic.Extensions.AspNetCore.Http.Middlewares`)

Sets the `Cache-Control` header per route prefix. It only sets the header if the response doesn't have one yet. The first matching route wins; if none matches, `DefaultHeader` is used.

```cs
using Microsoft.Net.Http.Headers;

app.UseCacheHeaders(options =>
{
    options.ConfigureNoCacheForRoutes(["/api"]);
    options.ConfigureValueForRoutes(new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromDays(30) }, ["/assets"]);
    options.DefaultHeader = new CacheControlHeaderValue { NoCache = true };
});
```

## HttpContextUserAccessor

Namespace: `Fusonic.Extensions.AspNetCore.Http`

Implementation of `IUserAccessor` (from `Fusonic.Extensions.Common`) that returns the user of the current `HttpContext`. It is required e.g. by the out-of-band processing of `Fusonic.Extensions.Hangfire`.

```cs
// Microsoft.Extensions.DependencyInjection
services.AddHttpContextAccessor();
services.AddScoped<IUserAccessor, HttpContextUserAccessor>();

// SimpleInjector (IHttpContextAccessor must be cross-wired from the IServiceCollection)
services.AddHttpContextAccessor();
container.Register<IUserAccessor, HttpContextUserAccessor>();
```

`User` throws an `InvalidOperationException` if there is no `HttpContext`; use `TryGetUser` for a non-throwing variant.

# Validation of Mediator requests

Namespace: `Fusonic.Extensions.AspNetCore.Validation`

Mediator requests can be validated with a simple decorator. Internally it uses the same validator that is used by ASP.NET Core for the request validation (`IObjectModelValidator`). This validator is registered by `services.AddControllers()` / `services.AddMvc()`. With SimpleInjector it must be resolvable from the container: `services.AddSimpleInjector(container, ...)` cross-wires framework components like this one automatically.

To enable Mediator request validation, add the decorator `RequestValidationDecorator` to `IRequestHandler`:

```cs
// SimpleInjector
container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(RequestValidationDecorator<,>));

// Microsoft.Extensions.DependencyInjection with Scrutor
// Register your handlers first, as Scrutor only decorates already registered services.
services.Decorate(typeof(IRequestHandler<,>), typeof(RequestValidationDecorator<,>));
```

When using the out-of-band decorators of [Fusonic.Extensions.Hangfire](https://github.com/fusonic/dotnet-extensions/blob/main/docs/Hangfire/README.md), register them before this decorator (`RegisterOutOfBandDecorators()` / `AddOutOfBandDecorators()`).

You can then use the `System.ComponentModel.DataAnnotations`-attributes and the `IValidatableObject`-interface for validating your models. When model validation fails, a `RequestValidationException` gets thrown. Its `Errors` property (`IDictionary<string, List<string>>`, property name to error messages) and `RequestType` contain the details. You might want to handle it in your [Exception filter](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#exception-filters), if you have one.

Example:
```cs
public class GetSomething : IQuery<IReadOnlyList<string>>, IValidatableObject
{
    [Required]
    public string Search { get; set; } = null!;

    [Range(1, 100)]
    public int MaxResults { get; set; } = 10;

    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From >= To)
            yield return new ValidationResult("From must be before To.", [nameof(From), nameof(To)]);
    }
}
```

This also works with records.

## OpenTelemetry

Namespace: `Fusonic.Extensions.AspNetCore.OpenTelemetry`

### Mediator tracing

To trace every request and notification sent by the Mediator package, register the tracing decorators:

```cs
// SimpleInjector: register the tracing decorators before the mediator.
container.RegisterMediatorTracingDecorators();
container.RegisterMediator( /* Mediator configuration */ );

// Microsoft.Extensions.DependencyInjection with Scrutor: register your handlers first,
// as Scrutor only decorates already registered services.
services.AddMediatorTracingDecorators();
```

When using the out-of-band decorators of [Fusonic.Extensions.Hangfire](https://github.com/fusonic/dotnet-extensions/blob/main/docs/Hangfire/README.md), register them before the tracing decorators (`RegisterOutOfBandDecorators()` / `AddOutOfBandDecorators()`).

`AddMediatorTracingDecorators` uses `TryDecorate`: if no handlers are registered yet, it silently does nothing. See the [Mediator documentation](https://github.com/fusonic/dotnet-extensions/blob/main/docs/Mediator/README.md) for how to register the handlers.

Also, when configuring the OpenTelemetry tracer, you need to enable the instrumentation:
```cs
services.AddOpenTelemetry()
        .WithTracing(tracer => tracer.AddMediatorInstrumentation());
```
