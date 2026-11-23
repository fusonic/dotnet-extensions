# Mediator

Package: `Fusonic.Extensions.Mediator` (namespace `Fusonic.Extensions.Mediator`)

- [Mediator](#mediator)
  - [Usage](#usage)
  - [Registration with SimpleInjector](#registration-with-simpleinjector)
    - [Transaction handling](#transaction-handling)
    - [Custom IMediator implementation](#custom-imediator-implementation)
  - [Registration with Microsoft.Extensions.DependencyInjection and Scrutor](#registration-with-microsoftextensionsdependencyinjection-and-scrutor)

A simple implementation of the mediator pattern, based on version 11.1.0 of the MediatR package. The pipelining was removed; use the decorators of your DI container instead.

- `ICommand`/`ICommand<TResponse>`, `IQuery<TResponse>`: use these instead of `IRequest` to distinguish commands from queries.
- `INotification`: published to all its `INotificationHandler<T>`.
- `IAsyncEnumerableRequest<T>`: streamed via `IAsyncEnumerableRequestHandler<TRequest, T>` and `IMediator.CreateAsyncEnumerable`.
- `OutOfBandAttribute`: put it on a handler class to run it as a background job, together with the out-of-band decorators of [Fusonic.Extensions.Hangfire](https://github.com/fusonic/dotnet-extensions/blob/main/docs/Hangfire/README.md). Only applies to handlers without a response (`Unit`) and to notification handlers.

## Usage

```cs
using Fusonic.Extensions.Mediator;

public record CreateUser(string Name) : ICommand<int>;

public class CreateUserHandler : IRequestHandler<CreateUser, int>
{
    public Task<int> Handle(CreateUser request, CancellationToken cancellationToken) => ...;
}

public record UserCreated(int Id) : INotification;

public class UserCreatedHandler : INotificationHandler<UserCreated>
{
    public Task Handle(UserCreated notification, CancellationToken cancellationToken) => ...;
}

// Inject IMediator (ISender and IPublisher are not registered)
var id = await mediator.Send(new CreateUser("Jane"));
await mediator.Publish(new UserCreated(id));
```

For commands without a response, implement `IRequestHandler<TRequest>` (returns `Task<Unit>`) or derive from `AsyncRequestHandler<TRequest>` (returns `Task`).

## Registration with SimpleInjector

The package references SimpleInjector with `PrivateAssets="all"`, so your project must reference `SimpleInjector` itself.

```cs
using Fusonic.Extensions.Mediator;

container.RegisterMediator(services, [typeof(Program).Assembly]);
```

`RegisterMediator` scans the given assemblies and registers all request, notification and async enumerable handlers as scoped, and `IMediator` as singleton. It also registers `IMediator` in the given `IServiceCollection` (resolved from the container), so it can be injected into services of the Microsoft DI container.

When using the out-of-band decorators of `Fusonic.Extensions.Hangfire`, call `container.RegisterOutOfBandDecorators()` before `RegisterMediator`.

### Transaction handling

By default `RegisterMediator` decorates all request and notification handlers with `TransactionalRequestHandlerDecorator<,>` and `TransactionalNotificationHandlerDecorator<>`, so they run within a `TransactionScope`. It also registers `ITransactionScopeHandler` (from `Fusonic.Extensions.Common.Transactions`). To disable this:

```cs
container.RegisterMediator(services, [typeof(Program).Assembly], o => o.EnableTransactionalDecorators = false);
```

### Custom IMediator implementation

`RegisterMediator` registers a `ServiceProviderMediator` resolving the handlers from the container. To use your own implementation:

```cs
container.Options.AllowOverridingRegistrations = true;
container.RegisterSingleton<IMediator, MyCustomMediator>();
```

## Registration with Microsoft.Extensions.DependencyInjection and Scrutor

There is no registration helper for Microsoft DI. Register the mediator and the handlers yourself, e.g. with Scrutor (referenced by your project). Unlike with SimpleInjector, the transactional decorators are opt-in.

```cs
using Fusonic.Extensions.Common.Transactions;
using Fusonic.Extensions.Mediator;
using Fusonic.Extensions.Mediator.DependencyInjection;

services.AddTransient<IMediator>(sp => new ServiceProviderMediator(sp));

services.Scan(s => s.FromAssemblyOf<Program>()
        .AddClasses(c => c.AssignableToAny(typeof(IRequestHandler<,>), typeof(INotificationHandler<>), typeof(IAsyncEnumerableRequestHandler<,>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime());

// When using Fusonic.Extensions.Hangfire: services.AddOutOfBandDecorators() must be called here, before any other handler decorator.

// Optional: Run all requests and notifications within a TransactionScope
services.TryDecorate(typeof(IRequestHandler<,>), typeof(TransactionalRequestHandlerDecorator<,>));
services.TryDecorate(typeof(INotificationHandler<>), typeof(TransactionalNotificationHandlerDecorator<>));
services.AddSingleton<ITransactionScopeHandler, TransactionScopeHandler>();
```

Scrutor only decorates services registered before the `Decorate`/`TryDecorate` call, so register the handlers first.
