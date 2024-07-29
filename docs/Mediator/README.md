# Mediator

This project contains a simple implementation of the common mediator pattern. The implementation focused on simplifying version 11.1.0 of the MediatR package. The pipelining was removed (SimpleInjector has a powerful built-in decorator feature) and a pure mediator implementation based on SimleInjector remains.

`ICommand`, `IQuery`: Use those instead of directly using the `IRequest` interfaces to clearly distinguish between commands and queries.  
`OutOfBandAttribute`: You can put this on your `Handler`-class. When used together with the [Hangfire-Decorator](../Hangfire/README.md), the execution of the handler will be queued as a background job automatically.  

# Registration

To register the mediator as well as your customer command/notification-handlers, use the following extenion where you can provide all the assemblies you want to scan for handlers.

```cs
container.RegisterMediator(services, [ typeof(Program).Assembly ]);
```

## Mediator transaction handling

When using the `RegisterMediator` method from above, all requests and notifications will run within a TransactionScope.
To disable this feature, configure as following:

```cs
container.RegisterMediator(services, [ typeof(Program).Assembly ], configure =>
{
    EnableTransactionalDecorators = false;
});
```

## Custom IMediator implementation

When using the `RegisterMediator` method from above, our default SimpleInjectorMediator will be registered.
If you want to use your custom mediator, configure as following:

```cs

container.Options.AllowOverridingRegistrations = true;
container.RegisterSingleton(typeof(IMediator), typeof(MyCustomMediator));

class MyCustomMediator : IMediator
{
  ...
}
```