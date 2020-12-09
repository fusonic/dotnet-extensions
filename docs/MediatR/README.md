# MediatR

This project contains some abstractions for MediatR.

`ICommand`, `IQuery`: Use those instead of directly using the `IRequest` interfaces to clearly distinguish between commands and queries.  
`OutOfBandAttribute`: You can put this on your `Handler`-class. When used together with the [Hangfire-Decorator](../Hangfire/README.md), the execution of the handler will be queued as a background job automatically.  

## MediatR transaction handling

There are decorators to run all MediatR-requests and notifications within a transaction.  
To enable this feature use the following SimpleInjector-Configuration:
```cs
Container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TransactionalRequestHandlerDecorator<,>));
Container.RegisterDecorator(typeof(INotificationHandler<>), typeof(TransactionNotificationHandlerDecorator<>));
Container.RegisterSingleton<ITransactionScopeHandler, TransactionScopeHandler>();
```