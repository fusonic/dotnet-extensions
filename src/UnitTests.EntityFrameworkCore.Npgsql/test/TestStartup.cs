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
        // Start PostgreSQL test container
        // - Reuse is enabled to speed up tests locally.
        //   Our CI pipelines always start fresh containers as the test jobs run within docker:dind and containers get disposed after a test run.
        // - The tmpfs mounts ensure that the database runs in memory only and does not write to disk, speeding up tests significantly.
        //   Also see https://www.fusonic.net/de/blog/fusonic-test-with-databases-part-3 for more information.
        container = new PostgreSqlBuilder("postgres:17-alpine")
            .WithReuse(true)
            .WithName("test.npgsql.extensions.unittests")
            .WithTmpfsMount("/var/lib/postgresql/data")
            .WithTmpfsMount("/dev/shm")
            .Build();

        await container.StartAsync();

        ConnectionString = new NpgsqlConnectionStringBuilder(container.GetConnectionString())
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