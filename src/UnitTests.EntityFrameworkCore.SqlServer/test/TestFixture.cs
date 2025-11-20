// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.Tests;

public class TestFixture : ServiceProviderTestFixture
{
    protected override void RegisterCoreDependencies(IServiceCollection services)
    {
        var testStore = new SqlServerDatabasePerTestStore(TestStartup.ConnectionString);
        services.AddSingleton<ITestStore>(testStore);

        AddDbContext(services, testStore);
    }

    protected virtual void AddDbContext(IServiceCollection services, SqlServerDatabasePerTestStore testStore)
    {
        services.AddDbContext<TestDbContext>(b => b.UseSqlServerDatabasePerTest(testStore));
    }
}
