using System;
using System.Threading.Tasks;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Fusonic.Extensions.UnitTests.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Fusonic.Extensions.UnitTests.Adapters.PostgreSql
{
    internal class PostgreSqlDatabaseProvider<TDbContext> : ITestDatabaseProvider<TDbContext>
        where TDbContext : DbContext
    {
        private readonly string connectionString;
        private readonly string? templateDb;
        private readonly Func<TDbContext, Task>? seed;
        private readonly string? dbName;
        private readonly DbContextOptions<TDbContext> options;

        private bool dbCreated;

        public PostgreSqlDatabaseProvider(
            string connectionString,
            string dbNamePrefix,
            string? templateDb,
            bool enableLogging,
            Action<NpgsqlDbContextOptionsBuilder>? optionsBuilder,
            Func<TDbContext, Task>? seed)
        {
            this.connectionString = connectionString;
            this.templateDb = templateDb;
            this.seed = seed;

            //The max identifier length of postgres is 63 chars. Minus the 22 we're using from the base64-guid the prefix must be max 41 chars.
            if (dbNamePrefix.Length > 41)
                throw new ArgumentException("The max. allowed length of the dbNamePrefix is 41 characters.");

            dbName = dbNamePrefix + Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=');
            string testDbConnectionString = PostgreSqlUtil.ReplaceDb(connectionString, dbName);

            var builder = new DbContextOptionsBuilder<TDbContext>().UseNpgsql(testDbConnectionString, optionsBuilder);

            if (enableLogging)
                builder.UseLoggerFactory(new LoggerFactory(new[] { new XunitLoggerProvider() }));

            options = builder.Options;
        }

        public void DropDb(TDbContext dbContext)
        {
            if (!dbCreated)
                return;

            dbCreated = false;

            //dropping can be done in the background
            var testDbName = dbName!;
            Task.Run(() => PostgreSqlUtil.DropDb(connectionString, testDbName));
        }

        public void SeedDb(TDbContext dbContext)
        {
            using (var connection = PostgreSqlUtil.CreatePostgresDbConnection(connectionString))
            using (var cmd = connection.CreateCommand())
            {
                connection.Open();

                //create the database
                cmd.CommandText = $"CREATE DATABASE \"{dbName}\"";
                if (templateDb != null)
                    cmd.CommandText += $" TEMPLATE \"{templateDb}\"";
                cmd.ExecuteNonQuery();

                dbCreated = true;
            }

            //if no template database was given, run the migrations
            if (templateDb == null)
                dbContext.Database.Migrate();

            //if a seed was set, run it
            seed?.Invoke(dbContext).Wait();
        }

        public DbContextOptions<TDbContext> GetContextOptions() => options;
    }
}