// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Common.Transactions;
using Fusonic.Extensions.Mediator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.Mediator.Tests;

public class ServiceProviderMediatorTransactionalTestFixture() : ServiceProviderMediatorTestFixture(true);
public class ServiceProviderMediatorNonTransactionalTestFixture() : ServiceProviderMediatorTestFixture(false);

public abstract class ServiceProviderMediatorTestFixture(bool enableTransactionalDecorators) : UnitTests.ServiceProviderTestFixture, IMediatorTestFixture
{
    public bool EnableTransactionalDecorators => enableTransactionalDecorators;

    protected override void RegisterCoreDependencies(IServiceCollection services)
    {
        base.RegisterCoreDependencies(services);

        services.AddTransient<IMediator>(sp => new ServiceProviderMediator(sp));

        services.Scan(s => s.FromAssemblyOf<ServiceProviderMediatorTestFixture>()
            .AddClasses(c => c.AssignableToAny(typeof(IRequestHandler<,>), typeof(INotificationHandler<>), typeof(IAsyncEnumerableRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        if (EnableTransactionalDecorators)
        {
            services.TryDecorate(typeof(IRequestHandler<,>), typeof(TransactionalRequestHandlerDecorator<,>));
            services.TryDecorate(typeof(INotificationHandler<>), typeof(TransactionalNotificationHandlerDecorator<>));
            services.AddSingleton<ITransactionScopeHandler, TransactionScopeHandler>();
        }
    }
}