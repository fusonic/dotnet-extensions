# Hangfire

Package: `Fusonic.Extensions.Hangfire` (namespace `Fusonic.Extensions.Hangfire`)

- [Out-of-band processing of commands and notifications](#out-of-band-processing-of-commands-and-notifications)
  - [Prerequisites](#prerequisites)
  - [Registration with SimpleInjector](#registration-with-simpleinjector)
  - [Registration with Microsoft.Extensions.DependencyInjection and Scrutor](#registration-with-microsoftextensionsdependencyinjection-and-scrutor)
  - [Custom job processor](#custom-job-processor)
- [Job display names](#job-display-names)
- [DisableHangfireDashboardAuthorizationFilter](#disablehangfiredashboardauthorizationfilter)

## Out-of-band processing of commands and notifications

Mark a handler of the [Mediator](https://github.com/fusonic/dotnet-extensions/blob/main/docs/Mediator/README.md) with `Fusonic.Extensions.Mediator.OutOfBandAttribute`. Instead of running the handler, the out-of-band decorator enqueues a Hangfire job that runs it in the background.
If the Hangfire storage enlists in the current transaction, the job is only scheduled when the transaction commits. This way the business operation and the follow-up commands/notifications are stored atomically.

```cs
using Fusonic.Extensions.Mediator;

[OutOfBand]
public class SendWelcomeEmailHandler : IRequestHandler<SendWelcomeEmail>
{
    public Task<Unit> Handle(SendWelcomeEmail request, CancellationToken cancellationToken) { ... }
}
```

Only handlers without a response (`IRequestHandler<TRequest, Unit>`, e.g. for `ICommand`/`IRequest`) and notification handlers are decorated. `[OutOfBand]` on a handler returning a response is ignored.

The job restores the culture, UI culture and user (via `IUserAccessor`) of the caller before running the handler. Inside the job, the out-of-band decorator is skipped, so the handler runs directly (still wrapped by the other decorators, e.g. the transactional decorators).

### Prerequisites

- Hangfire is configured, so `IBackgroundJobClient` can be resolved. With Microsoft DI use `services.AddHangfire(...)` from `Hangfire.NetCore` or `Hangfire.AspNetCore` (not referenced by this package).
- An `Fusonic.Extensions.Common.Security.IUserAccessor` is registered. It gets decorated with `HangfireUserAccessorDecorator`, which provides the user of the caller inside the job. With ASP.NET Core, `HttpContextUserAccessor` from `Fusonic.Extensions.AspNetCore` can be used.

### Registration with SimpleInjector

The out-of-band decorator must be the innermost decorator of a handler, so it can enqueue the actual handler type. SimpleInjector applies decorators in registration order, so call `RegisterOutOfBandDecorators()` **before** `RegisterMediator(...)` (which registers the transactional decorators) and before any other handler decorator.

```cs
using Fusonic.Extensions.Hangfire;
using Fusonic.Extensions.Mediator;
using Hangfire;

container.Register<IUserAccessor, YourUserAccessor>(Lifestyle.Scoped);

container.RegisterOutOfBandDecorators();
container.RegisterMediator(services, [typeof(Program).Assembly]);

// Hangfire resolves the JobProcessor from the container
services.AddHangfire(c => c
    .UseActivator(new ContainerJobActivator(container))
    .UsePostgreSqlStorage(...));

// IBackgroundJobClient must be resolvable from the container, e.g. via cross-wiring
// (SimpleInjector.Integration.ServiceCollection, plus UseSimpleInjector(container) on the built service provider)
services.AddSimpleInjector(container, o => o.AutoCrossWireFrameworkComponents = true);
```

### Registration with Microsoft.Extensions.DependencyInjection and Scrutor

Scrutor moves decorated registrations to keyed ones and hides their implementation type. Therefore register the handlers and the `IUserAccessor` first, then call `AddOutOfBandDecorators()`, then register all other handler decorators. It throws if an out-of-band handler is already decorated.

```cs
using Fusonic.Extensions.Common.Transactions;
using Fusonic.Extensions.Hangfire;
using Fusonic.Extensions.Mediator;
using Fusonic.Extensions.Mediator.DependencyInjection;

services.AddScoped<IUserAccessor, YourUserAccessor>();
services.AddTransient<IMediator>(sp => new ServiceProviderMediator(sp));

// 1. Handlers
services.Scan(s => s.FromAssemblyOf<Program>()
        .AddClasses(c => c.AssignableToAny(typeof(IRequestHandler<,>), typeof(INotificationHandler<>), typeof(IAsyncEnumerableRequestHandler<,>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime());

// 2. Out-of-band decorators
services.AddOutOfBandDecorators();

// 3. Other decorators
services.TryDecorate(typeof(IRequestHandler<,>), typeof(TransactionalRequestHandlerDecorator<,>));
services.TryDecorate(typeof(INotificationHandler<>), typeof(TransactionalNotificationHandlerDecorator<>));
services.AddSingleton<ITransactionScopeHandler, TransactionScopeHandler>();

services.AddHangfire(c => c.UsePostgreSqlStorage(...));
```

Notes:
- Handlers are detected by their `ServiceDescriptor.ImplementationType`. Handlers registered via factory or instance are not decorated.
- `AddOutOfBandDecorators()` re-registers the `IUserAccessor` as scoped. Do not inject it into singletons.
- Hangfire resolves the `ServiceProviderJobProcessor` from the `IServiceProvider` (`Hangfire.NetCore`/`Hangfire.AspNetCore`).

### Custom job processor

Both registrations accept a custom job processor:

```cs
container.RegisterOutOfBandDecorators(o => o.UseJobProcessor<MyJobProcessor>());   // derive from JobProcessor
services.AddOutOfBandDecorators(o => o.UseJobProcessor<MyJobProcessor>());         // derive from ServiceProviderJobProcessor
```

The job processor must implement `IJobProcessor`. `JobProcessorBase.ProcessAsync` is virtual.

## Job display names

Out-of-band jobs are shown as `OutOfBand Message: <message type> (<namespace>)` in the Hangfire dashboard. To show the handler type instead, use:

```cs
var dashboardOptions = new DashboardOptions { DisplayNameFunc = DashboardHelpers.FormatJobDisplayName };
app.UseHangfireDashboard(options: dashboardOptions);
```

## DisableHangfireDashboardAuthorizationFilter

By default the Hangfire dashboard only allows local requests. This fails when the app runs in a Docker container. For local development, where you have other means of authorization, disable the authorization completely:

```cs
var dashboardOptions = new DashboardOptions { Authorization = [new DisableHangfireDashboardAuthorizationFilter()] };
app.UseHangfireDashboard(options: dashboardOptions);
```
