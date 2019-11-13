using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    public interface ITestDatabaseProvider<TDbContext>
        where TDbContext : DbContext
    {
        /// <summary> Returns the options of the DbContext pointing to a usable database. </summary>
        DbContextOptions<TDbContext> GetContextOptions();

        /// <summary> Seeds the database. </summary>
        void SeedDb(TDbContext dbContext);

        /// <summary> Drops the database that was created. </summary>
        void DropDb(TDbContext dbContext);
    }
}