// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests;

public class TestDatabaseProvider : ITestDatabaseProvider
{
    public bool CreateDatabaseCalled { get; private set; }
    public bool DropDatabaseCalled { get; private set; }

    public string TestDbName { get; } = $"Test_{Guid.NewGuid():N}";

    public void CreateDatabase(DbContext dbContext)
    {
        CreateDatabaseCalled = true;
        dbContext.Database.EnsureCreated();
        dbContext.Database.Migrate();
    }

    public void DropDatabase(DbContext dbContext)
    {
        DropDatabaseCalled = true;
        dbContext.Database.EnsureDeleted();
    }
}