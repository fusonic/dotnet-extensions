// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.Tests;

public class SqlServerDbContextFactoryTests(SqlServerDbContextFactoryTests.FactoryFixture fixture) : SqlServerDatabasePerTestStoreTests<SqlServerDbContextFactoryTests.FactoryFixture>(fixture)
{
    public class FactoryFixture : TestFixture
    {
        protected override void AddDbContext(IServiceCollection services, SqlServerDatabasePerTestStore testStore)
        {
            services.AddDbContextFactory<TestDbContext>(b => b.UseSqlServerDatabasePerTest(testStore), ServiceLifetime.Scoped);
        }
    }
}
