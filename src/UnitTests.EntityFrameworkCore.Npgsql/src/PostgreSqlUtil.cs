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
    internal static async Task EnsureTemplateDbCreated(
        string connectionString,
        Func<string, Task> createTemplate,
        bool alwaysCreateTemplate = false)
    {
        await DatabaseTestHelper.EnsureTemplateDbCreated(
            connectionString, 
            CheckDatabaseExists, 
            DropDatabase, 
            NpgsqlConnection.ClearAllPools, 
            createTemplate, 
            alwaysCreateTemplate).ConfigureAwait(false);
    }

    /// <summary> Creates a test database template. </summary>
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
        var connection = new NpgsqlConnection(csBuilder.ConnectionString);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync().ConfigureAwait(false);

            // Drop existing Test-DB
            logger.LogInformation("Dropping database {Database}", dbName);
            await DropDatabase(connection, dbName).ConfigureAwait(false);

            // Create database
            logger.LogInformation("Creating database {Database}", dbName);
            await connection.ExecuteAsync($@"CREATE DATABASE ""{dbName}"" TEMPLATE template0 IS_TEMPLATE true").ConfigureAwait(false);

            // Migrate & run seed
            var options = new DbContextOptionsBuilder<TDbContext>()
                         .UseNpgsql(connectionString, npgsqlOptionsAction)
                         .LogTo(
                              (eventId, _) => eventId != RelationalEventId.CommandExecuted,
                              eventData => logger.Log(eventData.LogLevel, eventData.EventId, "[EF] {Message}", eventData.ToString()))
                         .Options;

            var dbContext = dbContextFactory(options);
            await using (dbContext.ConfigureAwait(false))
            {
                logger.LogInformation("Running migrations");
                await dbContext.Database.MigrateAsync().ConfigureAwait(false);

                if (seed != null)
                {
                    logger.LogInformation("Running seed");
                    await seed(dbContext).ConfigureAwait(false);
                }
            }

            //Convert to template
            logger.LogInformation("Setting connection limit on template");
            await connection.ExecuteAsync($@"ALTER DATABASE ""{dbName}"" CONNECTION LIMIT 0").ConfigureAwait(false);
        }

        NpgsqlConnection.ClearAllPools();
        logger.LogInformation("Done");
    }

    internal static async Task CreateDatabase(string templateConnectionString, string dbName)
    {
        var connection = CreatePostgresConnection(templateConnectionString, out var templateName);
        await using (connection.ConfigureAwait(false))
        {
            if (templateName == dbName)
                throw new ArgumentException("Template and new database name must not be the same.");

            await connection.OpenAsync().ConfigureAwait(false);
            await connection.ExecuteAsync($""" CREATE DATABASE "{dbName}" TEMPLATE "{templateName}"; """).ConfigureAwait(false);
        }
    }

    public static async Task DropDatabase(string connectionString)
    {
        var connection = CreatePostgresConnection(connectionString, out var dbName);
        await using (connection.ConfigureAwait(false))
        {
            connection.Open();
            await DropDatabase(connection, dbName).ConfigureAwait(false);
        }
    }

    private static async Task DropDatabase(NpgsqlConnection connection, string dbName)
    {
        if (await CheckDatabaseExists(connection, dbName).ConfigureAwait(false))
        {
            await connection.ExecuteAsync($@"ALTER DATABASE ""{dbName}"" IS_TEMPLATE false").ConfigureAwait(false);
            await connection.ExecuteAsync($@"DROP DATABASE ""{dbName}"" WITH (FORCE)").ConfigureAwait(false);
        }
    }

    public static async Task<bool> CheckDatabaseExists(string connectionString)
    {
        var connection = CreatePostgresConnection(connectionString, out var dbName);
        await using (connection.ConfigureAwait(false))
        {
            connection.Open();
            var exists = await CheckDatabaseExists(connection, dbName).ConfigureAwait(false);

            return exists;
        }
    }

    private static async Task<bool> CheckDatabaseExists(NpgsqlConnection connection, string dbName)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT EXISTS(SELECT * FROM pg_database WHERE datname=@dbName)";
        cmd.Parameters.Add("@dbName", NpgsqlDbType.Varchar).Value = dbName;
        
        var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        return result != null && (bool)result;
    }

    private static NpgsqlConnection CreatePostgresConnection(string connectionString, out string dbName)
    {
        var csBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(csBuilder.Database))
            throw new ArgumentException("Database name is missing in the connection string.", nameof(connectionString));

        dbName = csBuilder.Database;
        AssertNotPostgres(dbName);

        csBuilder.Database = "postgres";
        return new NpgsqlConnection(csBuilder.ConnectionString);
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