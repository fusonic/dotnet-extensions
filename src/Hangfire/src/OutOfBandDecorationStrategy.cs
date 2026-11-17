// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Fusonic.Extensions.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Fusonic.Extensions.Hangfire;

/// <summary> Decorates closed handler registrations of <paramref name="openServiceType"/> whose implementation is marked with <see cref="OutOfBandAttribute"/>. </summary>
internal sealed class OutOfBandDecorationStrategy(Type openServiceType, Func<Type, bool> canDecorate, Func<Type, Type> getDecoratorType)
    : DecorationStrategy(openServiceType, serviceKey: null)
{
    protected override bool CanDecorate(Type serviceType)
        => serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == ServiceType && canDecorate(serviceType);

    public override bool CanDecorate(ServiceDescriptor descriptor)
        => !descriptor.IsKeyedService
        && CanDecorate(descriptor.ServiceType)
        && descriptor.ImplementationType?.GetCustomAttribute<OutOfBandAttribute>() != null;

    public override Func<IServiceProvider, object?, object> CreateDecorator(Type serviceType, string serviceKey)
        => TypeDecorator(serviceType, serviceKey, getDecoratorType(serviceType));
}
