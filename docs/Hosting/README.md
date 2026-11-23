# Hosting

Package: `Fusonic.Extensions.Hosting`

- [Hosting](#hosting)
  - [TimedHostedService](#timedhostedservice)
    - [Setup](#setup)
    - [Configuration](#configuration)

## TimedHostedService

If you have a service that should execute every once in a while, for example a sync, you can use the `TimedHostedService` for triggering your service. You just have to care about your business logic, not about the hosting logic.

It has the following behaviour:
- The first run starts immediately when the host starts.
- The next run starts after the configured interval has passed since the **end** of the previous run (default: 15 minutes). Runs never overlap.
  - Example: Interval 15 minutes, the method takes 20 minutes. The runs start at 0, 35 and 70 minutes.
- Exceptions are logged and do not stop the service. The next run is scheduled as usual.
- After a successful run, an optional watchdog URI gets called (HTTP GET). That way you can monitor if your service is still running successfully.
- When the host stops, your method is notified via the provided `CancellationToken`.
- Each run resolves your service in a new scope.

### Setup

Given the following service:
```cs
public class HelloWorldService
{
    public Task Run(CancellationToken cancellationToken)
    {
        Console.WriteLine("Hello world");
        return LongRunningLogic(cancellationToken);
    }

    private Task LongRunningLogic(CancellationToken cancellationToken)
        => Task.Delay(10_000_000, cancellationToken);
}
```

Register it with `AddTimedHostedService<TService>`. The first parameter configures the settings, the second calls your method. The extensions are in the namespace `Fusonic.Extensions.Hosting`, the settings in `Fusonic.Extensions.Hosting.TimedHostedService`.

**Microsoft.Extensions.DependencyInjection**

The service gets registered as scoped:
```cs
using Fusonic.Extensions.Hosting;

services.AddTimedHostedService<HelloWorldService>(cfg => configuration.Bind("HelloWorld", cfg), (svc, ct) => svc.Run(ct));
```

**SimpleInjector**

Use the extension on the `AddSimpleInjector` options. The service gets registered with the container's default lifestyle and is resolved within an `AsyncScopedLifestyle` scope per run, so set `container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle()` if the service has scoped dependencies.

The hosted service requires an `IHttpClientFactory`, which you have to register yourself with `services.AddHttpClient()`:
```cs
using Fusonic.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient();
        services.AddSimpleInjector(container, options =>
        {
            options.AddTimedHostedService<HelloWorldService>(cfg => hostContext.Configuration.Bind("HelloWorld", cfg), (svc, ct) => svc.Run(ct));
        });
    })
    .Build();

host.Services.UseSimpleInjector(container);
```

### Configuration

`TimedHostedServiceSettings` has two options:
- `Interval`: Seconds between the end of a run and the start of the next one. Defaults to 900 (15 minutes). Must be greater than zero.
- `WatchdogUri`: URI which is called after each successful run. Defaults to `null` (no call).

The matching configuration in the `appsettings.json` could look like the following. All values are optional.
```json
{
    "HelloWorld": {
        "Interval": 300,
        "WatchdogUri": "https://watchdog.example.com/?projectId=12345"
    }
}
```
