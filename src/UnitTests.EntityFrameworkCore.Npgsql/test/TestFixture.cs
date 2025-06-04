// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.ServiceProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;

public class TestFixture : ServiceProviderTestFixture
{
    protected override void RegisterCoreDependencies(IServiceCollection services)
    {
        var testStoreOptions = new NpgsqlDatabasePerTestStoreOptions
        {
            TemplateCreator = CreateDatabase,
            ConnectionString = Configuration.GetConnectionString("Npgsql")!
        };

        var testStore = new NpgsqlDatabasePerTestStore(testStoreOptions);
        services.AddSingleton<ITestStore>(testStore);
        services.AddSingleton(testStoreOptions);

        services.AddDbContext<TestDbContext>(b => b.UseNpgsqlDatabasePerTest(testStore));
    }

    private static async Task CreateDatabase(string connectionString)
    {
        await using var dbContext = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().UseNpgsql(connectionString).Options);
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }
}