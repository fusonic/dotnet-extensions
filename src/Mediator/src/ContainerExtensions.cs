// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Fusonic.Extensions.Common.Transactions;
using Fusonic.Extensions.Mediator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Fusonic.Extensions.Mediator;

public static class ContainerExtensions
{
    /// <summary>
    /// Registers all <see cref="IRequestHandler{TRequest, TResponse}"/> and <see cref="INotificationHandler{TNotification}"/> in the given <paramref name="assemblies"/> collection.
    /// Also registers the <see cref="ServiceProviderMediator"/> as the mediator implementation for <see cref="IMediator"/>.
    /// </summary>
    public static void RegisterMediator(this Container container, IServiceCollection services, Assembly[] assemblies, Action<MediatorOptions>? configureOptions = null)
    {
        var options = new MediatorOptions();
        configureOptions?.Invoke(options);

        if (options.EnableTransactionalDecorators)
        {
            container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TransactionalRequestHandlerDecorator<,>), Lifestyle.Scoped);
            container.RegisterDecorator(typeof(INotificationHandler<>), typeof(TransactionalNotificationHandlerDecorator<>), Lifestyle.Scoped);
            container.RegisterSingleton<ITransactionScopeHandler, TransactionScopeHandler>();
        }

        container.RegisterSingleton<IMediator, SimpleInjectorMediator>();
        container.Register(typeof(IRequestHandler<,>), assemblies, Lifestyle.Scoped);
        container.Register(typeof(IAsyncEnumerableRequestHandler<,>), assemblies, Lifestyle.Scoped);
        container.Collection.Register(typeof(INotificationHandler<>), assemblies, Lifestyle.Scoped);

        services.AddSingleton(_ => container.GetInstance<IMediator>());
    }

    internal class SimpleInjectorMediator(Container container) : ServiceProviderMediator(container);
}