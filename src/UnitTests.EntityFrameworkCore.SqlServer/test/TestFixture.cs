// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.Tests;

public class TestFixture : ServiceProviderTestFixture
{
    protected override void RegisterCoreDependencies(IServiceCollection services)
    {
        var testStoreOptions = new SqlServerDatabasePerTestStoreOptions
        {
            TemplateCreator = CreateDatabase,
            ConnectionString = TestStartup.ConnectionString
        };

        var testStore = new SqlServerDatabasePerTestStore(testStoreOptions);
        services.AddSingleton<ITestStore>(testStore);
        services.AddSingleton(testStoreOptions);

        AddDbContext(services, testStore);
    }

    protected virtual void AddDbContext(IServiceCollection services, SqlServerDatabasePerTestStore testStore)
    {
        services.AddDbContext<TestDbContext>(b => b.UseSqlServerDatabasePerTest(testStore));
    }

    private static async Task CreateDatabase(string connectionString)
        => await SqlServerTestUtil.CreateTestDbTemplate<TestDbContext>(connectionString, o => new TestDbContext(o), useMigrations: false);
}
