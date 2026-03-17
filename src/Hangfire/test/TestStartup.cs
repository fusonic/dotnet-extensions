// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Dapper;
using Fusonic.Extensions.Hangfire.Tests;
using Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;
using Hangfire.PostgreSql;
using Npgsql;
using Testcontainers.PostgreSql;

[assembly: DapperAot(false)] // Pulled by Hangfire.PostgreSql
[assembly: AssemblyFixture(typeof(TestStartup))]

namespace Fusonic.Extensions.Hangfire.Tests;

public class TestStartup : IAsyncLifetime
{
    private PostgreSqlContainer? container;

    public static string ConnectionString { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // Start PostgreSQL test container
        container = new PostgreSqlBuilder("postgres:17-alpine")
            .WithReuse(true)
            .WithName("test.npgsql.extensions.hangfire")
            .WithTmpfsMount("/var/lib/postgresql/data")
            .WithTmpfsMount("/dev/shm")
            .Build();

        await container.StartAsync();

        ConnectionString = new NpgsqlConnectionStringBuilder(container.GetConnectionString())
        {
            Database = "hangfire"
        }.ConnectionString;

        // Create test database template
        await PostgreSqlUtil.CreateTestDbTemplate<TestDbContext>(
            ConnectionString,
            o => new TestDbContext(o),
            seed: async _ =>
            {
                await using var connection = new NpgsqlConnection(ConnectionString);
                await connection.OpenAsync();
                PostgreSqlObjectsInstaller.Install(connection);
            },
            useMigrations: false);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}