// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;
using Npgsql;
using Xunit.Sdk;
using Xunit.v3;

[assembly: TestPipelineStartup(typeof(TestStartup))]

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;

public class TestStartup : TestContainerStartup
{
    public static string ConnectionString { get; private set; } = null!;

    public override async ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        var container = await StartPostgreSql();
        ConnectionString = new NpgsqlConnectionStringBuilder(container.GetConnectionString())
        {
            Database = "efcore"
        }.ConnectionString;
    }
}