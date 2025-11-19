// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;

public class NpgsqlDbContextFactoryTests(NpgsqlDbContextFactoryTests.FactoryFixture fixture) : NpgsqlDatabasePerTestStoreTests<NpgsqlDbContextFactoryTests.FactoryFixture>(fixture)
{
    public class FactoryFixture : TestFixture
    {
        protected override void AddDbContext(IServiceCollection services, NpgsqlDatabasePerTestStore testStore)
        {
            services.AddDbContextFactory<TestDbContext>(b => b.UseNpgsqlDatabasePerTest(testStore), ServiceLifetime.Scoped);
        }
    }
}
