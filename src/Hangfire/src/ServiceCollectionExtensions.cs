// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Fusonic.Extensions.Common.Security;
using Fusonic.Extensions.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.Hangfire;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Decorates all request and notification handlers marked with <see cref="OutOfBandAttribute"/>, so they get enqueued as Hangfire jobs instead of being executed directly.
    /// Handlers and the <see cref="IUserAccessor"/> must be registered before calling this method and this method must be called before any other handler decorator is registered.
    /// Uses the <see cref="ServiceProviderJobProcessor"/> by default.
    /// </summary>
    public static IServiceCollection AddOutOfBandDecorators(this IServiceCollection services, Action<ContainerExtensions.OutOfBandOptions>? configure = null)
    {
        var options = new ContainerExtensions.OutOfBandOptions();
        configure?.Invoke(options);
        var jobProcessorType = options.JobProcessorType ?? typeof(ServiceProviderJobProcessor);

        EnsureOutOfBandHandlersAreNotDecorated(services);

        services.AddScoped(jobProcessorType);
        services.AddScoped<RuntimeOptions>();
        services.AddSingleton(OutOfBandNotificationHandlerRegistry.Create(services));

        services.TryDecorate(new OutOfBandDecorationStrategy(
            typeof(IRequestHandler<,>),
            t => t.GenericTypeArguments[1] == typeof(Unit),
            t => typeof(OutOfBandRequestHandlerDecorator<,>).MakeGenericType(t.GenericTypeArguments[0], jobProcessorType)));

        services.TryDecorate(new OutOfBandDecorationStrategy(
            typeof(INotificationHandler<>),
            _ => true,
            t => typeof(OutOfBandNotificationHandlerDecorator<,>).MakeGenericType(t.GenericTypeArguments[0], jobProcessorType)));

        DecorateUserAccessor(services);

        return services;
    }

    /// <summary>
    /// Scrutor moves decorated registrations to keyed ones, hiding their implementation type from later decorators.
    /// </summary>
    private static void EnsureOutOfBandHandlersAreNotDecorated(IServiceCollection services)
    {
        var decorated = services.FirstOrDefault(d => d.IsKeyedService
                                                  && d.ServiceType.IsGenericType
                                                  && d.ServiceType.GetGenericTypeDefinition() is var t && (t == typeof(IRequestHandler<,>) || t == typeof(INotificationHandler<>))
                                                  && d.KeyedImplementationType?.GetCustomAttribute<OutOfBandAttribute>() != null);

        if (decorated != null)
            throw new InvalidOperationException($"{decorated.ServiceType} is already decorated. Call {nameof(AddOutOfBandDecorators)} before registering other handler decorators.");
    }

    /// <summary> Decorates the <see cref="IUserAccessor"/> as scoped service, as the job processor sets the user per job. </summary>
    private static void DecorateUserAccessor(IServiceCollection services)
    {
        var index = FindLastIndex(services, typeof(IUserAccessor));
        if (index < 0)
            throw new InvalidOperationException($"No {nameof(IUserAccessor)} registered. Register it before calling {nameof(AddOutOfBandDecorators)}.");

        services.Decorate<IUserAccessor, HangfireUserAccessorDecorator>();
        services[index] = ServiceDescriptor.Scoped(typeof(IUserAccessor), services[index].ImplementationFactory!);
    }

    private static int FindLastIndex(IServiceCollection services, Type serviceType)
    {
        for (var i = services.Count - 1; i >= 0; i--)
        {
            if (services[i].ServiceType == serviceType && !services[i].IsKeyedService)
                return i;
        }

        return -1;
    }
}
