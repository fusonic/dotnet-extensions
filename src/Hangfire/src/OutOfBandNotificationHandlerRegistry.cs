// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.Hangfire;

/// <summary>
/// Resolves a specific notification handler (including its decorators) by its implementation type.
/// Stores the position of each handler within the registered handlers of its notification type.
/// Decorating services replaces the registrations in place, so the positions stay valid.
/// </summary>
internal sealed class OutOfBandNotificationHandlerRegistry(Dictionary<(Type ServiceType, Type ImplementationType), int> positions)
{
    public static OutOfBandNotificationHandlerRegistry Create(IServiceCollection services)
    {
        var positions = new Dictionary<(Type, Type), int>();
        var counts = new Dictionary<Type, int>();

        foreach (var descriptor in services)
        {
            if (descriptor.IsKeyedService
             || !descriptor.ServiceType.IsGenericType
             || descriptor.ServiceType.GetGenericTypeDefinition() != typeof(INotificationHandler<>))
            {
                continue;
            }

            var position = counts.GetValueOrDefault(descriptor.ServiceType);
            counts[descriptor.ServiceType] = position + 1;

            if (descriptor.ImplementationType != null)
                positions.TryAdd((descriptor.ServiceType, descriptor.ImplementationType), position);
        }

        return new OutOfBandNotificationHandlerRegistry(positions);
    }

    public object GetHandler(IServiceProvider serviceProvider, Type serviceType, Type implementationType)
    {
        if (!positions.TryGetValue((serviceType, implementationType), out var position))
            throw new InvalidOperationException($"Notification handler {implementationType} for {serviceType} is not registered.");

        return serviceProvider.GetServices(serviceType).ElementAt(position)!;
    }
}
