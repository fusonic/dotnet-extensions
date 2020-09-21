# Hangfire

- [Hangfire](#hangfire)
  - [DisableHangfireDashboardAuthorizationFilter](#disablehangfiredashboardauthorizationfilter)
  - [Transactional job processor](#transactional-job-processor)

## DisableHangfireDashboardAuthorizationFilter

For local development or when you have other means of authorization, you may want to disable the hangfire authorization for the dashboard.  

For disabling the local development authorization for local development, you usually just can use the default options. However, those may not work when running the backend in a docker container, as hangfire still filters the requests to "only local requests".
To completly disable any authorization use this filter instead.

Useage:
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