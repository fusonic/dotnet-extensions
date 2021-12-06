// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.InMemoryDatabase;

public static class DatabaseFixtureConfigurationExtensions
{
    /// <summary>
    /// Registers a provider to use in memory databases in unit tests. Use the [InMemoryTest]-attribute on class- or method-level to use that provider.
    /// </summary>
    public static DatabaseFixtureConfiguration<TDbContext> UseInMemoryDatabase<TDbContext>(this DatabaseFixtureConfiguration<TDbContext> configuration, Func<TDbContext, Task>? seed = null)
        where TDbContext : DbContext
    {
        configuration.RegisterProvider(typeof(InMemoryTestAttribute), _ => new InMemoryDatabaseProvider<TDbContext>(seed));
        return configuration;
    }
}
