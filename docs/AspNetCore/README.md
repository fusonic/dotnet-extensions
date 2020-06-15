# ASP .Net Core Extensions

- [ASP .Net Core Extensions](#asp-net-core-extensions)
  - [MediatR transaction handling](#mediatr-transaction-handling)
  - [ServiceCollection extensions](#servicecollection-extensions)
  - [CultureUtil](#cultureutil)
  - [Ignore paths Middleware](#ignore-paths-middleware)

## MediatR transaction handling

There are decorators to run all MediatR-requests and notifications within a transaction.

To enable this feature use the following SimpleInjector-Configuration:
```cs
Container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TransactionCommandHandlerDecorator<,>));
Container.RegisterDecorator(typeof(INotificationHandler<>), typeof(TransactionNotificationHandlerDecorator<>));
```

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
