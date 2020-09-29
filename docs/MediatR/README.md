# MediatR

This project contains some abstractions for MediatR.

`ICommand`, `IQuery`: Use those instead of directly using the `IRequest` interfaces to clearly distinguish between commands and queries.
`OutOfBandAttribute`: You can put this on your `Handler`-class. When used together with the [Hangfire-Decorator](../Hangfire/README.md), the execution of the handler will be queued as a background job automatically.
`TransactionalDecorator`: Those decorators spawn a transaction over all command-, query- and notification-handlers. You can register them like follows:

```cs
Container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TransactionCommandHandlerDecorator<,>));
Container.RegisterDecorator(typeof(INotificationHandler<>), typeof(TransactionNotificationHandlerDecorator<>));
Container.RegisterSingleton<ITransactionScopeHandler, TransactionScopeHandler>();
```
