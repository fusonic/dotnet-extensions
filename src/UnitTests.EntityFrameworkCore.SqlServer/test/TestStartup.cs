// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.Tests;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

[assembly: AssemblyFixture(typeof(TestStartup))]

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.Tests;

public class TestStartup : IAsyncLifetime
{
    private MsSqlContainer? container;
    public static string ConnectionString { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // In CI the database is provided as a service. Locally, a test container is started.
        // - Reuse is enabled to speed up tests locally.
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Mssql");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
                .WithReuse(true)
                .WithName("test.mssql.extensions.unittests")
                .Build();

            await container.StartAsync();
            connectionString = container.GetConnectionString();
        }

        ConnectionString = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "efcore"
        }.ConnectionString;

        // Create test database template
        await SqlServerTestUtil.CreateTestDbTemplate<TestDbContext>(
            ConnectionString,
            o => new TestDbContext(o),
            useMigrations: false);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}