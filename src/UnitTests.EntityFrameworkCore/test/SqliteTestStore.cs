// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests;

public class SqliteTestStore : ITestStore, IDisposable
{
    private SqliteConnection? connection;

    public bool CreateDatabaseCalled { get; private set; }
    public bool DropDatabaseCalled { get; private set; }

    public string TestDbName { get; private set; } = null!;
    public string ConnectionString => $"Data Source={TestDbName};Mode=Memory;Cache=Shared";

    public async Task CreateDatabase()
    {
        if (CreateDatabaseCalled)
            return;

        CreateDatabaseCalled = true;
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public void DropDatabase()
    {
        DropDatabaseCalled = true;
        using var dbContext = CreateDbContext();
        dbContext.Database.EnsureDeleted();
    }

    private TestDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<TestDbContext>()
              .UseSqlite(ConnectionString)
              .AddInterceptors(new ConnectionOpeningInterceptor(CreateDatabase))
              .Options);

    public void OnTestConstruction()
    {
        TestDbName = $"Test_{Guid.NewGuid():N}";
        CreateDatabaseCalled = false;
        DropDatabaseCalled = false;

        // Need to maintain an open connection spanning a test to avoid dropping the in-memory DB.
        connection = new SqliteConnection(ConnectionString);
        connection.Open();
    }

    public async Task OnTestEnd()
    {
        DropDatabase();
        connection?.Close();
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}