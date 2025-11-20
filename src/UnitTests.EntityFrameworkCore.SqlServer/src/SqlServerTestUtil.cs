// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer;

public static class SqlServerTestUtil
{
    public static async Task<bool> CheckDatabaseExists(string connectionString)
    {
        var connection = CreateMasterConnection(connectionString, out var dbName);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync().ConfigureAwait(false);
            return await DbExists(dbName, connection).ConfigureAwait(false);
        }
    }

    private static async Task<bool> DbExists(string dbName, SqlConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM sys.databases WHERE name = N'{dbName}'";
        return (int)(await cmd.ExecuteScalarAsync().ConfigureAwait(false) ?? 0) > 0;
    }

    internal static async Task CreateDatabase(string templateConnectionString, string dbName, string dataDirectoryPath)
    {
        var connection = CreateMasterConnection(templateConnectionString, out var templateName);
        await using (connection.ConfigureAwait(false))
        {
            if (templateName == dbName)
                throw new ArgumentException("Template and new database name must not be the same.");

            await connection.OpenAsync().ConfigureAwait(false);

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"""
                RESTORE DATABASE [{dbName}] FROM DISK='{templateName}.bak'
                WITH
                MOVE '{templateName}' TO '{dataDirectoryPath}/{dbName}.mdf',
                MOVE '{templateName}_log' TO '{dataDirectoryPath}/{dbName}_log.ldf'
                """;

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    public static async Task DropDatabase(string connectionString)
    {
        var connection = CreateMasterConnection(connectionString, out var dbName);
        await using (connection.ConfigureAwait(false))
        {
            connection.Open();
            await DropDatabase(dbName, connection).ConfigureAwait(false);
        }
    }

    internal static async Task DropDatabase(string dbName, SqlConnection connection)
    {
        var cmd = connection.CreateCommand();

        // Other connections users may still access the DB. Set it to single user to disconnect other sessions and drop it then.
        SqlConnection.ClearAllPools();
        cmd.CommandText = $"""
            DECLARE @SQL nvarchar(1000);
            IF EXISTS (SELECT 1 FROM sys.databases WHERE [name] = N'{dbName}')
            BEGIN
                SET @SQL = N'USE [{dbName}];
        
                             ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                             USE [tempdb];
        
                             DROP DATABASE [{dbName}];';
                EXEC (@SQL);
            END;
            """;

        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    /// <summary> Creates a test database template. </summary>
    /// <param name="connectionString">
    ///     Connection string to the test database template. The database does not have to exist.
    ///     The initial catalog must be set and determines the name of the template database.
    /// </param>
    /// <param name="dbContextFactory">Return a DbContext using the given options.</param>
    /// <param name="sqlServerOptionsAction">The configuration action for .UseSqlServer().</param>
    /// <param name="seed">Optional seed action that gets executed after creating the database.</param>
    /// <param name="logger">Logger. Defaults to console logger.</param>
    /// <param name="useMigrations">
    /// If there are database migrations to be executed when setting up the test database, set this to true. <br/>
    /// If the database and the tables should be created from the current state, set this to false. <br/>
    /// Defaults to true.
    /// </param>
    /// <param name="overwrite">When true, the template gets dropped and recreated if it exists. When false, the template will only be created if it does not exist. Defaults to false.</param>
    public static async Task CreateTestDbTemplate<TDbContext>(
        string connectionString,
        Func<DbContextOptions<TDbContext>, TDbContext> dbContextFactory,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null,
        Func<TDbContext, Task>? seed = null,
        ILogger? logger = null,
        bool useMigrations = true,
        bool overwrite = false)
        where TDbContext : DbContext
    {
        logger ??= CreateConsoleLogger();

        var options = new DbContextOptionsBuilder<TDbContext>()
                     .UseSqlServer(connectionString, sqlServerOptionsAction)
                     .LogTo(
                          (eventId, _) => eventId != RelationalEventId.CommandExecuted,
                          eventData => logger.Log(eventData.LogLevel, eventData.EventId, "[EF] {Message}", eventData.ToString()))
                     .Options;

        if (!overwrite)
        {
            var exists = await CheckDatabaseExists(connectionString).ConfigureAwait(false);
            if (exists)
            {
                logger.LogInformation("Database template already exists. Skipping creation.");
                return;
            }
        }

        // Drop existing test template
        await DropDatabase(connectionString).ConfigureAwait(false);

        var dbContext = dbContextFactory(options);
        await using (dbContext.ConfigureAwait(false))
        {
            // Migrate and run seed
            if (useMigrations)
            {
                logger.LogInformation("Running migrations");
                await dbContext.Database.MigrateAsync().ConfigureAwait(false);
            }
            else
            {
                logger.LogInformation("Creating database");
                await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
            }

            if (seed != null)
            {
                logger.LogInformation("Running seed");
                await seed(dbContext).ConfigureAwait(false);
            }
        }

        var connection = CreateMasterConnection(connectionString, out var dbName);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync().ConfigureAwait(false);
            var cmd = connection.CreateCommand();

            // Create backup
            cmd.CommandText = $"BACKUP DATABASE {dbName} TO DISK='{dbName}.bak' WITH INIT";
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        logger.LogInformation("Done");
    }

    private static SqlConnection CreateMasterConnection(string connectionString, out string dbName)
    {
        var csBuilder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(csBuilder.InitialCatalog))
            throw new ArgumentException("InitialCatalog is missing in the connection string.", nameof(connectionString));

        dbName = csBuilder.InitialCatalog;
        AssertNotMaster(dbName);

        csBuilder.InitialCatalog = "master";
        return new SqlConnection(csBuilder.ConnectionString);
    }

    private static void AssertNotMaster([NotNull] string? dbName)
    {
        if (string.IsNullOrWhiteSpace(dbName))
            throw new ArgumentException("DB Name is empty.", nameof(dbName));

        if ("master".Equals(dbName.ToLower(CultureInfo.InvariantCulture).Trim(), StringComparison.Ordinal))
            throw new ArgumentException("You can't do this on the master catalog.");
    }

    private static ILogger CreateConsoleLogger()
        => LoggerFactory.Create(b => b.AddSimpleConsole(c => c.SingleLine = true))
                        .CreateLogger(nameof(SqlServerTestUtil));
}