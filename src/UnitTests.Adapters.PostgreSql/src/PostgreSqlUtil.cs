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

namespace Fusonic.Extensions.UnitTests.Adapters.PostgreSql;

/// <summary>
/// Utilities for Postgres tests. This class is can be used from external sources, like LinqPad or PowerShell scripts
/// </summary>
public static class PostgreSqlUtil
{
    private static readonly Regex GetDatabaseRegex = new("Database=([^;]+)", RegexOptions.Compiled);

    /// <summary>
    /// Drops all test databases with the given prefix. Excludes those from deleting that continue with one of the strings in the exclude-parameter.
    /// Example:
    ///   Prefix = Test_
    ///   Exclude = branch1, branch2
    /// Deletes all databases beginning with Test_ except those beginning with Test_branch1 and Test_branch2.
    /// 
    /// </summary>
    /// <param name="connectionString">Connection string to the postgres database.</param>
    /// <param name="dbPrefix">Prefix of the test databases that should be dropped.</param>
    /// <param name="exclude">Excludes databases from deleting that continue with one of the strings in the exclude-parameter.</param>
    /// <param name="dryRun">Does not drop the databases. Only outputs the databases that would get dropped.</param>
    /// <param name="logger">Logger. Defaults to console logger.</param>
    public static void Cleanup(string connectionString, string dbPrefix, IEnumerable<string>? exclude = null, bool dryRun = false, ILogger? logger = null)
    {
        AssertNotPostgres(dbPrefix);
        logger ??= CreateConsoleLogger();

        using var connection = CreatePostgresDbConnection(connectionString);

        var dbNames = connection.Query<string>($"SELECT datname FROM pg_database WHERE datname LIKE '{dbPrefix}%'");

        var ignoreDbs = exclude?.Select(e => dbPrefix + e).ToList() ?? new List<string>();
        var databases = dbNames.Where(dbName => !ignoreDbs.Any(dbName.StartsWith)).ToList();

        if (dryRun)
        {
            logger.LogInformation("[DryRun] Would drop the following databases: {Databases}", Environment.NewLine + string.Join(Environment.NewLine, databases));
        }
        else
        {
            var version = GetServerVersion(connection);
            Parallel.ForEach(databases, dbName =>
            {
                logger.LogInformation("Dropping {Database}", dbName);
                DropDb(connectionString, dbName, version);
            });
        }
    }

    /// <summary>
    /// Drops the given database. If there are still users connected to the database their sessions will be terminated.
    /// </summary>
    /// <param name="connectionString">Connection string to the postgres database.</param>
    /// <param name="dbName">Database that should be dropped.</param>
    /// <param name="version">Version of the PostgreSQL server if known. If this is not set, the version will be queried.
    /// Starting with PG13, FORCE will be used when dropping a database.</param>
    public static void DropDb(string connectionString, string dbName, Version? version = null)
    {
        AssertNotPostgres(dbName);

        using var connection = CreatePostgresDbConnection(connectionString);

        var exists = connection.ExecuteScalar<bool?>($"SELECT true FROM pg_database WHERE datname='{dbName}'");
        if (exists != true)
            throw new InvalidOperationException($"DropDb: Database {dbName} does not exist.");

        connection.Execute($@"ALTER DATABASE ""{dbName}"" IS_TEMPLATE false");

        version ??= GetServerVersion(connection);
        if (version.Major >= 13)
        {
            connection.Execute($@"DROP DATABASE ""{dbName}"" WITH (FORCE)");
        }
        else
        {
            // Disallow new connections and terminate all connected user sessions on the database before dropping
            connection.Execute($@"ALTER DATABASE ""{dbName}"" CONNECTION LIMIT 0");
            TerminateUsers(dbName, connection);
            connection.Execute($@"DROP DATABASE ""{dbName}""");
        }
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

        //Clear connection pools when coming from LinqPad. Otherwise consecutive calls may cause exceptions in Migrate() as it tries to reuse a terminated connection.
        NpgsqlConnection.ClearAllPools();

        //Open connection to the postgres-DB (for drop, create, alter)
        using var connection = CreatePostgresDbConnection(connectionString);

        //get database from connection string.
        var dbName = GetDatabaseName(connectionString);
        AssertNotPostgres(dbName);

        //Drop existing Test-DB
        if (connection.ExecuteScalar<bool>($"SELECT EXISTS(SELECT * FROM pg_catalog.pg_database WHERE datname='{dbName}')"))
        {
            logger.LogInformation("Dropping database {Database}", dbName);
            DropDb(connectionString, dbName);
        }

        //Create database
        logger.LogInformation("Creating database {Database}", dbName);
        connection.Execute($@"CREATE DATABASE ""{dbName}"" TEMPLATE template0 IS_TEMPLATE true");

        //Migrate & run seed
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
        Console.WriteLine("Setting connection limit on template");
        connection.Execute($@"ALTER DATABASE ""{dbName}"" CONNECTION LIMIT 0");
        TerminateUsers(dbName, connection);

        Console.WriteLine("Done");
    }

    /// <summary> Creates a database from a given template. </summary>
    public static void CreateDatabase(string connectionString, string dbName, string? template = null)
    {
        using var connection = CreatePostgresDbConnection(connectionString);
        connection.Execute($@"CREATE DATABASE ""{dbName}"" TEMPLATE ""{template ?? "template0"}""");
    }

    /// <summary> Creates a connection using the given connection string, but replacing the database with postgres. </summary>
    private static NpgsqlConnection CreatePostgresDbConnection(string connectionString)
        => new(ReplaceDatabaseName(connectionString, "postgres"));

    /// <summary> Replaces the database in a connection string with another one. </summary>
    public static string ReplaceDatabaseName(string connectionString, string dbName)
        => GetDatabaseRegex.Replace(connectionString, $"Database={dbName}");

    /// <summary> Returns the database name in the connection string or null, if it could not be matched. </summary>
    public static string? GetDatabaseName(string connectionString)
    {
        var match = GetDatabaseRegex.Match(connectionString);
        if (!match.Success || match.Groups.Count != 2)
            return null;

        return match.Groups[1].Value;
    }

    /// <summary> Gets the version of the PostgreSQL server. </summary>
    public static Version GetServerVersion(string connectionString)
    {
        using var connection = CreatePostgresDbConnection(connectionString);
        return GetServerVersion(connection);
    }

    private static Version GetServerVersion(NpgsqlConnection connection)
    {
        var serverVersion = connection.ExecuteScalar<string>("SHOW server_version");
        return Version.TryParse(serverVersion, out var version) ? version : new Version();
    }

    private static void TerminateUsers(string dbName, NpgsqlConnection postgresDbConnection) => postgresDbConnection.Execute(
        $@"SELECT pg_terminate_backend(pg_stat_activity.pid)
           FROM pg_stat_activity
           WHERE pg_stat_activity.datname = '{dbName}'
           AND pid <> pg_backend_pid()");

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