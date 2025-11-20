// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.Tests;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit.Sdk;
using Xunit.v3;

[assembly: TestPipelineStartup(typeof(TestStartup))]

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.Tests;

public class TestStartup : ITestPipelineStartup
{
    private MsSqlContainer? container;
    public static string ConnectionString { get; private set; } = null!;

    public async ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        // Start MSSQL test container
        // - Reuse is enabled to speed up tests locally.
        //   Our CI pipelines always start fresh containers as the test jobs run within docker:dind and containers get disposed after a test run.
        container = new MsSqlBuilder()
            .WithReuse(true)
            .WithName("test.mssql.extensions.unittests")
            .Build();

        await container.StartAsync();

        ConnectionString = new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = "efcore"
        }.ConnectionString;

        // Create test database template
        await SqlServerTestUtil.CreateTestDbTemplate<TestDbContext>(
            ConnectionString,
            o => new TestDbContext(o),
            useMigrations: false);
    }

    public ValueTask StopAsync() => ValueTask.CompletedTask;
}