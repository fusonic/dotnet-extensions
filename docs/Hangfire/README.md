# Hangfire

- [Hangfire](#hangfire)
  - [DisableHangfireDashboardAuthorizationFilter](#disablehangfiredashboardauthorizationfilter)
  - [Transactional job processor](#transactional-job-processor)

## Out-Of-Band Processing of CQRS Command and Event-Handlers

When applying CQRS, it allows us to easily decorate handlers with generic decorators.
By applying the "[Fusonic.Extensions.Common.OutOfBand]" attribute, you can decorate handlers which are allowed to run outside of the current flow. Whenever this handler gets called, the execution pipeline looks for the attribute on the handler. If the attribute is available, the handler is scheduled for async execution, meaning it runs “out of band” of the current logical flow.
That means that the message will be stored in the outbox as part of the current ACID transaction and so it gets scheduled for async background processing as soon as the current transaction completes.

This way we you can atomically perform your business operation including scheduling commands/events which must be executed afterwards, so that we get into an consistent state.

Registration:
```cs
Container.RegisterOutOfBandDecorators();
Container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TransactionalRequestHandlerDecorator<,>));
```

Usage:
```cs
[OutOfBand]
public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand>
{
     ...
}
```


## DisableHangfireDashboardAuthorizationFilter

For local development or when you have other means of authorization, you may want to disable the hangfire authorization for the dashboard.  

For disabling the local development authorization for local development, you usually just can use the default options. However, those may not work when running the backend in a docker container, as hangfire still filters the requests to "only local requests".
To completly disable any authorization use this filter instead.

Usage:
```cs
dashboardOptions = new DashboardOptions { Authorization = new[] { new DisableHangfireDashboardAuthorizationFilter() } };
app.UseHangfireDashboard(options: dashboardOptions);
```

## Transactional job processor

If you want all your background jobs to run within a transaction (which is usually the case), you can use the `TransactionalJobProcessor`.

Configuration with SimpleInjector:
```cs
Container.RegisterSingleton<ITransactionScopeHandler, TransactionScopeHandler>();

// Transaction scope for all request handlers:
Container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TransactionalCommandHandlerDecorator<,>));

// Transaction scope for all notification handlers:
Container.RegisterDecorator(typeof(INotificationHandler<>), typeof(TransactionalNotificationHandlerDecorator<>));
```

## Using DisplayNameFunc.DisplayNameFunc

OutOfBand jobs are processed via `IJobProcessor.ProcessAsync`. As a consequence, each jobs name in hangfires dashboard will be listed as "JobProcessor.ProcessAsync".  
Fortunately there is an easy way to enable meaningful job display names:

```cs
dashboardOptions.DisplayNameFunc = DashboardHelpers.FormatJobDisplayName;
```

Once enabled, request handlers type name (without assembly information) will be used as the job display name.