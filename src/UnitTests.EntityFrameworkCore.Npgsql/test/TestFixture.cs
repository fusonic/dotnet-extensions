// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;

public class TestFixture : ServiceProviderTestFixture
{
    protected override void RegisterCoreDependencies(IServiceCollection services)
    {
        var testStore = new NpgsqlDatabasePerTestStore(TestStartup.ConnectionString);
        services.AddSingleton<ITestStore>(testStore);
        AddDbContext(services, testStore);
    }

    protected virtual void AddDbContext(IServiceCollection services, NpgsqlDatabasePerTestStore testStore)
    {
        services.AddDbContext<TestDbContext>(b => b.UseNpgsqlDatabasePerTest(testStore));
    }
}