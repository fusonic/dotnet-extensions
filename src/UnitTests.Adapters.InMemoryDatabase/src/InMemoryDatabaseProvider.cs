// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fusonic.Extensions.UnitTests.Adapters.InMemoryDatabase;

internal class InMemoryDatabaseProvider<TDbContext> : ITestDatabaseProvider<TDbContext>
    where TDbContext : DbContext
{
    private static readonly string DbPrefix = typeof(TDbContext).FullName + "_";

    private readonly Func<TDbContext, Task>? seed;
    private readonly string dbName = DbPrefix + "_" + Guid.NewGuid();

    public InMemoryDatabaseProvider(Func<TDbContext, Task>? seed) => this.seed = seed;

    public DbContextOptions<TDbContext> GetContextOptions()
        => new DbContextOptionsBuilder<TDbContext>()
          .UseInMemoryDatabase(dbName)
          .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
          .Options;

    public void SeedDatabase(TDbContext dbContext)
    {
        //For some weird reason any async access to the dbContext causes some kind of task deadlock. The cause for it is the AsyncTestSyncContext from XUnit.
        //It causes .Wait() to lock indefinitely. It doesn't relate to the connection or the migration.
        //Seems somehow connected to dbContext.SaveChangesAsync() when called in the seed.
        //Running the seed with an extra Task.Run() around works...
        Task.Run(() => seed?.Invoke(dbContext).Wait()).Wait();
    }

    public void DropDatabase(TDbContext dbContext) => dbContext.Database.EnsureDeleted();
    public void CreateDatabase(TDbContext dbContext) => dbContext.Database.EnsureCreated();
}