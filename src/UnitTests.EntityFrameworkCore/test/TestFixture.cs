// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.SimpleInjector;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SimpleInjector;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests;

public class TestFixture : UnitTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        container.RegisterDbContext<TestDbContext, TestDatabaseProvider>((dbName, builder) => builder.UseSqlite(GetConnectionString(dbName)));

        // Need to maintain an open connection spanning a test to avoid dropping the in-memory DB.
        container.RegisterTestScoped(() =>
        {
            var dbName = container.GetInstance<TestDatabaseProvider>().TestDbName;
            var connection = new SqliteConnection(GetConnectionString(dbName));
            connection.Open();
            return connection;
        });
    }

    private static string GetConnectionString(string dbName) => $"Data Source={dbName};Mode=Memory;Cache=Shared";
}