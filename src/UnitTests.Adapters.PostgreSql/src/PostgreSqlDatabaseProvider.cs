// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Fusonic.Extensions.XUnit.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Polly;

namespace Fusonic.Extensions.UnitTests.Adapters.PostgreSql;

internal class PostgreSqlDatabaseProvider<TDbContext> : ITestDatabaseProvider<TDbContext>
    where TDbContext : DbContext
{
    private static Version? pgVersion;

    private readonly string connectionString;
    private readonly string? templateDb;
    private readonly Func<TDbContext, Task>? seed;
    private readonly string dbName;
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
        var testDbConnectionString = PostgreSqlUtil.ReplaceDatabaseName(connectionString, dbName);

        var builder = new DbContextOptionsBuilder<TDbContext>().UseNpgsql(testDbConnectionString, npgsqlOptions);
        dbContextOptions?.Invoke(builder);

        if (enableLogging)
            builder.UseLoggerFactory(new LoggerFactory(new[] { new XUnitLoggerProvider() }));

        builder.AddInterceptors(new CreateDatabaseInterceptor<TDbContext>(CreateDatabase));

        options = builder.Options;
    }

    public void DropDatabase(TDbContext dbContext)
    {
        if (!dbCreated)
            return;

        dbCreated = false;

        pgVersion ??= PostgreSqlUtil.GetServerVersion(connectionString);
        PostgreSqlUtil.DropDb(connectionString, dbName, pgVersion);
    }

    private static readonly object TemplateSync = new();
    public void CreateDatabase(TDbContext dbContext)
    {
        if (dbCreated)
            return;

        // Creating a DB from a template can cause an exception when done in parallel.
        // The lock usually prevents this, however, we still encounter race conditions
        // where we just have to retry.
        // 55006: source database "test_template" is being accessed by other users
        Policy.Handle<NpgsqlException>(e => e.SqlState == "55006")
              .WaitAndRetry(15, _ => TimeSpan.FromSeconds(1))
              .Execute(CreateDatabase);

        void CreateDatabase()
        {
            lock (TemplateSync)
            {
                if (dbCreated)
                    return;

                PostgreSqlUtil.CreateDatabase(connectionString, dbName, templateDb);

                //if no template database was given, run the migrations
                if (templateDb == null)
                    dbContext.Database.Migrate();

                //For some weird reason any async access to the dbContext causes some kind of task deadlock. The cause for it is the AsyncTestSyncContext from XUnit.
                //It causes .Wait() to lock indefinitely. It doesn't relate to the connection or the migrate above.
                //Seems somehow connected to dbContext.SaveChangesAsync() when called in the seed. (at least in my tests).
                //Running the seed with an extra Task.Run() around works...
                Task.Run(() => seed?.Invoke(dbContext).Wait()).Wait();

                dbCreated = true;
            }
        }
    }

    public void SeedDatabase(TDbContext dbContext)
    {
        // We're doing this in an interceptor on the first DB access using CreateDatabase()
    }

    public DbContextOptions<TDbContext> GetContextOptions() => options;
}
