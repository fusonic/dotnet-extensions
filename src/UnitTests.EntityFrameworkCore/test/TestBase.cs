// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Data.Sqlite;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests;

public abstract class TestBase : DatabaseUnitTest<TestDbContext, TestFixture>
{
    protected TestBase(TestFixture fixture) : base(fixture) =>
        // Need to ensure that one connection is open for the test, as otherwise the InMemory-DB would be dropped.
        _ = GetInstance<SqliteConnection>();

    protected override void DropTestDatabase() => Query(ctx => GetInstance<TestDatabaseProvider>().DropDatabase(ctx));
}