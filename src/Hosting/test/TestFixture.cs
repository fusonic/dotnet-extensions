// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using Fusonic.Extensions.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Fusonic.Extensions.Hosting.Tests
{
    public class TestFixture : UnitTestFixture
    {
        private IServiceProvider serviceProvider = null!;

        protected sealed override void RegisterCoreDependencies(Container container)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSimpleInjector(container);

            serviceProvider = services.BuildServiceProvider()
                                      .UseSimpleInjector(container);
        }
    }
}