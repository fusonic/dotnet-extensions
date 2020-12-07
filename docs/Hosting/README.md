# Hosting

- [Hosting](#hosting)
  - [TimedHostedService](#timedhostedservice)
    - [Setup](#setup)

## TimedHostedService

If you have a service that should execute every once in a while, for example a sync, you can use `TimedHostedService<>` for triggering your service. You just have to care about your business logic, not about the hosting logic.

The `TimedHostedService<>` is a hosted service which executes a method in your service class, repeating after a specified interval.

It has the following behaviour:
- Execute a method on your service repeatedly
- The interval between executions can be configured, defaulting at 15 minutes.
- It ensures, that your method is not called concurrently.
  - If your method takes longer than the interval to execute, it will not be triggered again.
  - Example: Method takes 20 minutes
    - First trigger after 15 minutes sees that your method is still running => nothing happens
    - Second trigger after 30 minutes sees that your method is not running => method gets startet again
- After your method executed successfully, a watchdog URL gets called. That way you can monitor if your service is still running successfully.
  - The URL is optional 
- When the program gets stopped, your service is kindly notified to shutdown with the provided CancellationToken.

### Setup
Given the following Service:
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

When configuring your host you can register timed services in the `AddSimpleInjector`-part. There's an extension to the options `AddTimedHostedService<TService>`. It has a parameter for the configuration and one to call your method.  
The configuration only consists of two options:
- `Interval`: The interval in seconds in which your method should be executed. Defaults to 900 (15 minutes).
- `WatchdogUri`: The URI which should be called after a successfull run. Defaults to `null`.

Example configuration:
```cs
var host = new HostBuilder()
   .ConfigureHostConfiguration(/* ... */)
   .ConfigureServices((hostContext, services) => {
       // Some config

       services.AddSimpleInjector(container, options =>
       {
           // Add HelloWorldService
           // Configuration is in the appsettings-section "HelloWorld"
           // We want to run our service method "Run" and we react to the provided cancellation token in there.
           options.AddTimedHostedService<HelloWorldService>(cfg => hostContext.Configuration.Bind("HelloWorld", cfg), (svc, ct) => svc.Run(ct));
       });
       // Some more config
    })
   .Build()
```

The according configuration in the appettings for this example could look like the following. Note that all parameters are optional and the config could also be injected via EnvironmentVariables, depending on your HostConfiguration.
```cs
{
    "HelloWorld": {
        "Interval": 300,
        "WatchdogUrl": "https://watchdog.fusonic.net/?projectId=12345"
    }
}
```