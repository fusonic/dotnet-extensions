// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Threading.Tasks;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fusonic.Extensions.UnitTests.Adapters.InMemoryDatabase
{
    internal class InMemoryDatabaseProvider<TDbContext> : ITestDatabaseProvider<TDbContext>
        where TDbContext : DbContext
    {
        private static readonly string dbPrefix = typeof(TDbContext).FullName + "_";

        private readonly Func<TDbContext, Task>? seed;
        private readonly string dbName = dbPrefix + "_" + Guid.NewGuid();

        public InMemoryDatabaseProvider(Func<TDbContext, Task>? seed)
        {
            this.seed = seed;
        }

        public DbContextOptions<TDbContext> GetContextOptions()
        {
            return new DbContextOptionsBuilder<TDbContext>()
                  .UseInMemoryDatabase(dbName)
                  .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                  .Options;
        }

        public void SeedDb(TDbContext dbContext)
        {
            //For some weird reason any async access to the dbContext causes some kind of task deadlock. The cause for it is the AsyncTestSyncContext from XUnit.
            //It causes .Wait() to lock indefinitely. It doesn't relate to the connection or the migrate above.
            //Seems somehow connected to dbContext.SaveChangesAsync() when called in the seed. (at least in my tests).
            //Running the seed with an extra Task.Run() around works...
            Task.Run(() => seed?.Invoke(dbContext).Wait()).Wait();
        }

        public void DropDb(TDbContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
        }
    }
}