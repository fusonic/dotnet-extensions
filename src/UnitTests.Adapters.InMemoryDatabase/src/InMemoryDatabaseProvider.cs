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
            seed?.Invoke(dbContext).Wait();
        }

        public void DropDb(TDbContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
        }
    }
}