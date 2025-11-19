// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.Tests;
using Microsoft.Data.SqlClient;
using Xunit.Sdk;
using Xunit.v3;

[assembly: TestPipelineStartup(typeof(TestStartup))]

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.Tests;

public class TestStartup : TestContainerStartup
{
    public static string ConnectionString { get; private set; } = null!;

    public override async ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        var container = await StartMsSql();
        ConnectionString = new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = "efcore"
        }.ConnectionString;
    }
}