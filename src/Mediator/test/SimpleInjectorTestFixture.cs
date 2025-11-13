// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Fusonic.Extensions.Mediator.Tests;

public class SimpleInjectorMediatorTransactionalTestFixture() : SimpleInjectorMediatorTestFixture(true);
public class SimpleInjectorMediatorNonTransactionalTestFixture() : SimpleInjectorMediatorTestFixture(false);

public abstract class SimpleInjectorMediatorTestFixture(bool enableTransactionalDecorators) : UnitTests.SimpleInjectorTestFixture, IMediatorTestFixture
{
    public bool EnableTransactionalDecorators => enableTransactionalDecorators;

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

    private void RegisterMediator(Container container, IServiceCollection services)
    {
        container.RegisterMediator(services, [typeof(SimpleInjectorMediatorTestFixture).Assembly], o => o.EnableTransactionalDecorators = EnableTransactionalDecorators);
    }
}
