// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests;

public class CreateDatabaseInterceptorTests : TestBase
{
    public CreateDatabaseInterceptorTests(TestFixture fixture) : base(fixture)
    { }

    [Fact]
    public void NoDatabaseAccessInTest_DoesNotCreateDatabase()
    {
        Query(_ =>
        {
            // No database access, only resolving DbContext
        });

        ((SqliteTestStore)GetInstance<ITestStore>()).CreateDatabaseCalled.Should().BeFalse();
    }

    [Fact]
    public void DatabaseAccess_CreatesDatabase()
    {
        Query(ctx =>
        {
            ctx.TestEntities.Add(new TestEntity { Name = "Test" });
            ctx.SaveChanges();
        });

        ((SqliteTestStore)GetInstance<ITestStore>()).CreateDatabaseCalled.Should().BeTrue();
    }
}