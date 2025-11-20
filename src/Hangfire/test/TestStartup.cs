// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Hangfire.Tests;
using Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;
using Hangfire.PostgreSql;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit.Sdk;
using Xunit.v3;

[assembly: TestPipelineStartup(typeof(TestStartup))]

namespace Fusonic.Extensions.Hangfire.Tests;

public class TestStartup : ITestPipelineStartup
{
    private PostgreSqlContainer? container;

    public static string ConnectionString { get; private set; } = null!;

    public async ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        // Start PostgreSQL test container
        container = new PostgreSqlBuilder()
            .WithReuse(true)
            .WithImage("postgres:17")
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

    public ValueTask StopAsync() => ValueTask.CompletedTask;
}