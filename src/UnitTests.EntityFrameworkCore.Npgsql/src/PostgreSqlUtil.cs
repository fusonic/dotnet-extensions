// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using NpgsqlTypes;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

public static class PostgreSqlUtil
{
    /// <summary> Creates a test database. </summary>
    /// <param name="connectionString">Connection string to the test database. The database does not have to exist.</param>
    /// <param name="dbContextFactory">Returns a DbContext using the given options.</param>
    /// <param name="npgsqlOptionsAction">The configuration action for .UseNpgsql().</param>
    /// <param name="seed">Optional seed action that gets executed after creating the database.</param>
    /// <param name="logger">Logger. Defaults to console logger.</param>
    public static async Task CreateTestDbTemplate<TDbContext>(
        string connectionString,
        Func<DbContextOptions<TDbContext>, TDbContext> dbContextFactory,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null,
        Func<TDbContext, Task>? seed = null,
        ILogger? logger = null)
        where TDbContext : DbContext
    {
        logger ??= CreateConsoleLogger();

        // Get database from connection string.
        var csBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var dbName = csBuilder.Database;
        AssertNotPostgres(dbName);

        // Open connection to the postgres-DB (for drop, create, alter)
        csBuilder.Database = "postgres";
        await using (var connection = new NpgsqlConnection(csBuilder.ConnectionString))
        {
            await connection.OpenAsync();

            // Drop existing Test-DB
            if (await CheckDatabaseExists(connection, dbName))
            {
                logger.LogInformation("Dropping database {Database}", dbName);

                await connection.ExecuteAsync($@"ALTER DATABASE ""{dbName}"" IS_TEMPLATE false");
                await connection.ExecuteAsync($@"DROP DATABASE ""{dbName}"" WITH (FORCE)");
            }

            // Create database
            logger.LogInformation("Creating database {Database}", dbName);
            await connection.ExecuteAsync($@"CREATE DATABASE ""{dbName}"" TEMPLATE template0 IS_TEMPLATE true");

            // Migrate & run seed
            var options = new DbContextOptionsBuilder<TDbContext>()
                         .UseNpgsql(connectionString, npgsqlOptionsAction)
                         .LogTo(
                              (eventId, _) => eventId != RelationalEventId.CommandExecuted,
                              eventData => logger.Log(eventData.LogLevel, eventData.EventId, "[EF] {Message}", eventData.ToString()))
                         .Options;

            await using (var dbContext = dbContextFactory(options))
            {
                logger.LogInformation("Running migrations");
                await dbContext.Database.MigrateAsync();

                if (seed != null)
                {
                    logger.LogInformation("Running seed");
                    await seed(dbContext);
                }
            }

            //Convert to template
            logger.LogInformation("Setting connection limit on template");
            await connection.ExecuteAsync($@"ALTER DATABASE ""{dbName}"" CONNECTION LIMIT 0");
        }

        NpgsqlConnection.ClearAllPools();
        logger.LogInformation("Done");
    }

    public static async Task<bool> CheckDatabaseExists(string connectionString)
    {
        var csBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var dbName = csBuilder.Database!;

        csBuilder.Database = "postgres";
        await using var connection = new NpgsqlConnection(csBuilder.ConnectionString);
        connection.Open();
        var exists = await CheckDatabaseExists(connection, dbName);

        return exists;
    }

    private static async Task<bool> CheckDatabaseExists(NpgsqlConnection connection, string dbName)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT EXISTS(SELECT * FROM pg_database WHERE datname=@dbName)";
        cmd.Parameters.Add("@dbName", NpgsqlDbType.Varchar).Value = dbName;

        var result = await cmd.ExecuteScalarAsync();
        return result != null && (bool)result;
    }

    private static void AssertNotPostgres([NotNull] string? dbName)
    {
        if (string.IsNullOrWhiteSpace(dbName))
            throw new ArgumentException("DB Name is empty.", nameof(dbName));

        if ("postgres".Equals(dbName.ToLower(CultureInfo.InvariantCulture).Trim(), StringComparison.Ordinal))
            throw new ArgumentException("You can't do this on the postgres database.");
    }

    private static ILogger CreateConsoleLogger()
        => LoggerFactory.Create(b => b.AddSimpleConsole(c => c.SingleLine = true))
                        .CreateLogger(nameof(PostgreSqlUtil));
}