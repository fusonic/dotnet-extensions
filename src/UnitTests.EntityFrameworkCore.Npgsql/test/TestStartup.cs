// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;
using Npgsql;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(TestStartup))]

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;

public class TestStartup : IAsyncLifetime
{
    private PostgreSqlContainer? container;

    public static string ConnectionString { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // In CI the database is provided as a service. Locally, a test container is started.
        // - Reuse is enabled to speed up tests locally.
        // - The tmpfs mounts ensure that the database runs in memory only and does not write to disk, speeding up tests significantly.
        //   Also see https://www.fusonic.net/de/blog/fusonic-test-with-databases-part-3 for more information.
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            container = new PostgreSqlBuilder("postgres:17-alpine")
                .WithReuse(true)
                .WithName("test.npgsql.extensions.unittests")
                .WithTmpfsMount("/var/lib/postgresql/data")
                .WithTmpfsMount("/dev/shm")
                .Build();

            await container.StartAsync();
            connectionString = container.GetConnectionString();
        }

        ConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "efcore"
        }.ConnectionString;

        // Create test database template
        await PostgreSqlUtil.CreateTestDbTemplate<TestDbContext>(
            ConnectionString, 
            o => new TestDbContext(o),
            useMigrations: false);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}