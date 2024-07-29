// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.SimpleInjector;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Fusonic.Extensions.Mediator.Tests;

public class TestFixture : SimpleInjectorTestFixture
{
    protected override void RegisterCoreDependencies(Container container)
    {
        base.RegisterCoreDependencies(container);
        var services = new ServiceCollection();

        services.AddSimpleInjector(container, setup =>
        {
            setup.AutoCrossWireFrameworkComponents = true;
            RegisterMediator(container, services);
        });

        services.BuildServiceProvider().UseSimpleInjector(container);
    }

    private static void RegisterMediator(Container container, IServiceCollection services)
    {
        container.RegisterMediator(services, [typeof(TestFixture).Assembly]);
    }
}