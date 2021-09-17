// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Fusonic.Extensions.UnitTests.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Fusonic.Extensions.UnitTests.Adapters.PostgreSql
{
    internal class PostgreSqlDatabaseProvider<TDbContext> : ITestDatabaseProvider<TDbContext>
        where TDbContext : DbContext
    {
        private static Version? pgVersion;

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

            builder.AddInterceptors(new CreateDatabaseInterceptor(CreateDb));

            options = builder.Options;
        }

        public void DropDb(TDbContext dbContext)
        {
            if (!dbCreated)
                return;

            dbCreated = false;

            pgVersion ??= PostgreSqlUtil.GetVersion(connectionString);
            PostgreSqlUtil.DropDb(connectionString, dbName!, pgVersion);
        }

        private static readonly object templateSync = new();
        private void CreateDb(TDbContext dbContext)
        {
            // Creating a DB from a template can cause an exception when done in parallel.
            // This lock prevents this.
            // 55006: source database "test_template" is being accessed by other users
            lock (templateSync)
            {
                using var connection = PostgreSqlUtil.CreatePostgresDbConnection(connectionString);
                using var cmd = connection.CreateCommand();

                connection.Open();

                cmd.CommandText = $"CREATE DATABASE \"{dbName}\"";
                if (templateDb != null)
                    cmd.CommandText += $" TEMPLATE \"{templateDb}\"";

                cmd.ExecuteNonQuery();
            }

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

        public void SeedDb(TDbContext dbContext)
        {
            // We're doing this in an interceptor on the first DB access using CreateDb()
        }

        public DbContextOptions<TDbContext> GetContextOptions() => options;

        private class CreateDatabaseInterceptor : DbConnectionInterceptor
        {
            private readonly object sync = new();

            private readonly Action<TDbContext> createDb;
            private bool dbCreated;

            public CreateDatabaseInterceptor(Action<TDbContext> createDb) => this.createDb = createDb;

            public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
            {
                CreateDatabase(eventData.Context);
                return base.ConnectionOpening(connection, eventData, result);
            }

            public override ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = new CancellationToken())
            {
                CreateDatabase(eventData.Context);
                return base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
            }

            private void CreateDatabase(DbContext dbContext)
            {
                lock(sync)
                {
                    if (dbCreated)
                        return;

                    createDb((TDbContext)dbContext);

                    dbCreated = true;
                }
            }
        }
    }
}