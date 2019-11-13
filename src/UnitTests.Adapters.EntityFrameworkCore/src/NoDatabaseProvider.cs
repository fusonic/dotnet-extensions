using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    public class NoDatabaseProvider<TDbContext> : ITestDatabaseProvider<TDbContext>
        where TDbContext : DbContext
    {
        public DbContextOptions<TDbContext> GetContextOptions()
            => new DbContextOptions<TDbContext>();

        public void SeedDb(TDbContext dbContext)
        { }

        public void DropDb(TDbContext dbContext)
        { }
    }
}