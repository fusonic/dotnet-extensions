using System;
using System.Threading.Tasks;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Fusonic.Extensions.UnitTests.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
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
            Func<TDbContext, Task>? seed,
            Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptions,
            Action<DbContextOptionsBuilder<TDbContext>>? dbContextOptions)
        {
            this.connectionString = connectionString;
            this.templateDb = templateDb;
            this.seed = seed;

            //The max identifier length of postgres is 63 chars. Minus the 22 we're using from the base64-guid the prefix must be max 41 chars.
            if (dbNamePrefix.Length > 41)
                throw new ArgumentException("The max. allowed length of the dbNamePrefix is 41 characters.");

            dbName = dbNamePrefix + Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=');
            string testDbConnectionString = PostgreSqlUtil.ReplaceDb(connectionString, dbName);

            var builder = new DbContextOptionsBuilder<TDbContext>().UseNpgsql(testDbConnectionString, npgsqlOptions);
            dbContextOptions?.Invoke(builder);

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

                int retry = 0;
                while (!dbCreated)
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                        dbCreated = true;
                    }
                    catch (PostgresException e)
                    {
                        retry++;
                        //Exception: Npgsql.PostgresException : 55006: source database "xyz" is being accessed by other users
                        //Usually an error, as "xyz" should be a template and templates can't be accessed by other users (except postgres)
                        //However, timescale tends to launch a background worker connection on the template when restoring from it, which blocks us from creating the template.
                        //The background worker usually stops pretty fast again and a retry the succeeds.
                        //There is an open ticket for it.
                        //TODO: Remove retry logic once https://github.com/timescale/timescaledb/issues/1593 is resolved
                        if (retry >= 5 || e.SqlState != "55006")
                            throw;
                        
                        Task.Delay(500).Wait();
                    }
                }
            }

            //if no template database was given, run the migrations
            if (templateDb == null)
                dbContext.Database.Migrate();

            //For some weird reason any async access to the dbContext causes some kind of task deadlock. The cause for it is the AsyncTestSyncContext from XUnit.
            //It causes .Wait() to lock indefinitely. It doesn't relate to the connection or the migrate above.
            //Seems somehow connected to dbContext.SaveChangesAsync() when called in the seed. (at least in my tests).
            //Running the seed with an extra Task.Run() around works...
            Task.Run(() => seed?.Invoke(dbContext).Wait()).Wait();
        }

        public DbContextOptions<TDbContext> GetContextOptions() => options;
    }
}