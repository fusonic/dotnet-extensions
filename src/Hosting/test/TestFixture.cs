// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.SimpleInjector;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Fusonic.Extensions.Hosting.Tests;

public class TestFixture : SimpleInjectorTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSimpleInjector(container);

        _ = services.BuildServiceProvider().UseSimpleInjector(container);
    }
}