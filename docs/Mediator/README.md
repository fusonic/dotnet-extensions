# Mediator

This project contains a simple implementation of the common mediator pattern. The implementation focused on simplifying version 11.1.0 of the MediatR package. The pipelining was removed (SimpleInjector has a powerful built-in decorator feature) and a pure mediator implementation based on SimleInjector remains.

`ICommand`, `IQuery`: Use those instead of directly using the `IRequest` interfaces to clearly distinguish between commands and queries.  
`OutOfBandAttribute`: You can put this on your `Handler`-class. When used together with the [Hangfire-Decorator](../Hangfire/README.md), the execution of the handler will be queued as a background job automatically.  

# Registration with SimpleInjector

To register the mediator as well as your customer command/notification-handlers, use the following extension where you can provide all the assemblies you want to scan for handlers.

```cs
// When using SimpleInjector
container.RegisterMediator(services, [ typeof(Program).Assembly ]);
```

Note that SimpleInjector does not come as transient build dependency and must be referenced in the project.

## Mediator transaction handling

By default all requests and notifications will run within a TransactionScope.
This feature can be disabled by setting the `EnableTransactionalDecorators`-property to false:

```cs
container.RegisterMediator(services, [ typeof(Program).Assembly ], configure =>
{
    EnableTransactionalDecorators = false;
});
```

## Custom IMediator implementation

When using the `RegisterMediator` method from above, our default DependencyInjectionMediator will be registered.
If you want to use your custom mediator, configure as following:

```cs

container.Options.AllowOverridingRegistrations = true;
container.RegisterTransient(typeof(IMediator), typeof(MyCustomMediator));

class MyCustomMediator : IMediator
{
  ...
}
```

# Registration with Microsoft.Extensions.DependencyInjection and Scrutor

When using the default Microsoft DI container together with Scrutor, you can register the mediator and your handlers as follows:

```cs
services.AddTransient<IMediator>(sp => new ServiceProviderMediator(sp));

services.Scan(s => s.FromAssemblyOf<Program>()
        .AddClasses(c => c.AssignableToAny(typeof(IRequestHandler<,>), typeof(INotificationHandler<>), typeof(IAsyncEnumerableRequestHandler<,>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime());

// Optional: Run all requests and notifications within a TransactionScope
services.TryDecorate(typeof(IRequestHandler<,>), typeof(TransactionalRequestHandlerDecorator<,>));
services.TryDecorate(typeof(INotificationHandler<>), typeof(TransactionalNotificationHandlerDecorator<>));
services.AddSingleton<ITransactionScopeHandler, TransactionScopeHandler>();
```