// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using SimpleInjector;

namespace Fusonic.Extensions.UnitTests.SimpleInjector;

public static class ContainerExtensions
{
    private static readonly Lifestyle TestScopedLifestyle = new TestScopedLifestyle();

    public static void RegisterTestScoped<TConcrete>(this Container container)
        where TConcrete : class
        => container.Register<TConcrete>(TestScopedLifestyle);

    public static void RegisterTestScoped<TService, TServiceImplementation>(this Container container)
        where TServiceImplementation : class, TService
        where TService : class
        => container.Register<TService, TServiceImplementation>(TestScopedLifestyle);

    public static void RegisterTestScoped<TService>(this Container container, Func<TService> instanceCreator)
        where TService : class
        => container.Register(instanceCreator, TestScopedLifestyle);
}
