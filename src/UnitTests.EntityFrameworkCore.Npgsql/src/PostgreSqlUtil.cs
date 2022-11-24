// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

public static partial class PostgreSqlUtil
{
    private static readonly Regex DatabaseRegex = GetDatabaseRegex();

    // <summary> Returns the database name in the connection string or null, if it could not be matched. </summary>
    public static string? GetDatabaseName(string connectionString)
    {
        var match = DatabaseRegex.Match(connectionString);
        if (!match.Success || match.Groups.Count != 2)
            return null;

        return match.Groups[1].Value.Trim();
    }

    /// <summary> Creates a test database. </summary>
    /// <param name="connectionString">Connection string to the test database. The database does not have to exist.</param>
    /// <param name="dbContextFactory">Returns a DbContext using the given options.</param>
    /// <param name="npgsqlOptionsAction">The configuration action for .UseNpgsql().</param>
    /// <param name="seed">Optional seed action that gets executed after creating the database.</param>
    /// <param name="logger">Logger. Defaults to console logger.</param>
    public static void CreateTestDbTemplate<TDbContext>(
        string connectionString,
        Func<DbContextOptions<TDbContext>, TDbContext> dbContextFactory,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null,
        Func<TDbContext, Task>? seed = null,
        ILogger? logger = null)
        where TDbContext : DbContext
    {
        logger ??= CreateConsoleLogger();

        // Open connection to the postgres-DB (for drop, create, alter)
        using var connection = CreatePostgresDbConnection(connectionString);

        // Get database from connection string.
        var dbName = GetDatabaseName(connectionString);
        AssertNotPostgres(dbName);

        // Drop existing Test-DB
        if (connection.ExecuteScalar<bool>("SELECT EXISTS(SELECT * FROM pg_catalog.pg_database WHERE datname=@dbName)", new { dbName }))
        {
            logger.LogInformation("Dropping database {Database}", dbName);
            DropDb(connectionString, dbName);
        }

        // Create database
        logger.LogInformation("Creating database {Database}", dbName);
        connection.Execute($@"CREATE DATABASE ""{dbName}"" TEMPLATE template0 IS_TEMPLATE true");

        // Migrate & run seed
        var options = new DbContextOptionsBuilder<TDbContext>()
                     .UseNpgsql(connectionString, npgsqlOptionsAction)
                     .LogTo(
                          (eventId, _) => eventId != RelationalEventId.CommandExecuted,
                          eventData => logger.Log(eventData.LogLevel, eventData.EventId, "[EF] {Message}", eventData.ToString()))
                     .Options;
        using (var dbContext = dbContextFactory(options))
        {
            logger.LogInformation("Running migrations");
            dbContext.Database.Migrate();

            if (seed != null)
            {
                logger.LogInformation("Running seed");
                seed(dbContext).Wait();
            }
        }

        //Convert to template
        logger.LogInformation("Setting connection limit on template");
        connection.Execute($@"ALTER DATABASE ""{dbName}"" CONNECTION LIMIT 0");

        logger.LogInformation("Done");
    }

    /// <summary> Drops the given database. </summary>
    /// <param name="connectionString">Connection string to the postgres database.</param>
    /// <param name="dbName">Database that should be dropped.</param>
    public static void DropDb(string connectionString, string dbName)
    {
        AssertNotPostgres(dbName);

        using var connection = CreatePostgresDbConnection(connectionString);
        var exists = connection.ExecuteScalar<bool>("SELECT EXISTS(SELECT * FROM pg_database WHERE datname=@dbName)", new { dbName });
        if (!exists)
            return;

        connection.Execute($@"ALTER DATABASE ""{dbName}"" IS_TEMPLATE false");
        connection.Execute($@"DROP DATABASE ""{dbName}"" WITH (FORCE)");
    }

    private static void AssertNotPostgres([NotNull] string? dbName)
    {
        if (string.IsNullOrWhiteSpace(dbName))
            throw new ArgumentException("DB Name is empty.", nameof(dbName));

        if ("postgres".Equals(dbName.ToLower(CultureInfo.InvariantCulture).Trim(), StringComparison.Ordinal))
            throw new ArgumentException("You can't do this on the postgres database.");
    }

    /// <summary> Creates a connection using the given connection string, but replacing the database with postgres. </summary>
    internal static NpgsqlConnection CreatePostgresDbConnection(string connectionString)
        => new(ReplaceDatabaseName(connectionString, "postgres"));

    /// <summary> Replaces the database in a connection string with another one. </summary>
    internal static string ReplaceDatabaseName(string connectionString, string dbName)
        => DatabaseRegex.Replace(connectionString, $"Database={dbName}");

    private static ILogger CreateConsoleLogger()
        => LoggerFactory.Create(b => b.AddSimpleConsole(c => c.SingleLine = true))
                        .CreateLogger(nameof(PostgreSqlUtil));

    [GeneratedRegex("Database=([^;]+)")]
    private static partial Regex GetDatabaseRegex();
}