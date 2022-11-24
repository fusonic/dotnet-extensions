// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

public class NpgsqlTestDatabaseProvider : ITestDatabaseProvider
{
    private readonly NpgsqlTestDatabaseProviderSettings settings;
    private readonly string templateName;

    private bool dbCreated;

    /// <inheritdoc />
    public string TestDbName { get; } = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=');

    public NpgsqlTestDatabaseProvider(NpgsqlTestDatabaseProviderSettings settings)
    {
        this.settings = settings;

        if (string.IsNullOrWhiteSpace(settings.TemplateConnectionString))
            throw new ArgumentException($"The {nameof(NpgsqlTestDatabaseProviderSettings)}.{nameof(NpgsqlTestDatabaseProviderSettings.TemplateConnectionString)} is empty. This is a required setting.");

        templateName = PostgreSqlUtil.GetDatabaseName(settings.TemplateConnectionString)
                    ?? throw new ArgumentException($"Could not find database in the {nameof(NpgsqlTestDatabaseProviderSettings.TemplateConnectionString)}.");
    }

    /// <inheritdoc />
    void ITestDatabaseProvider.CreateDatabase(DbContext dbContext) => CreateDatabase();

    /// <inheritdoc cref="ITestDatabaseProvider.CreateDatabase(DbContext)"/>
    public void CreateDatabase()
    {
        if (dbCreated)
            return;

        // Creating a DB from a template can cause an exception when done in parallel.
        // The lock usually prevents this, however, we still encounter race conditions
        // where we just have to retry.
        // 55006: source database "test_template" is being accessed by other users
        Policy.Handle<NpgsqlException>(e => e.SqlState == "55006")
              .WaitAndRetry(30, _ => TimeSpan.FromMilliseconds(500))
              .Execute(CreateDb);

        void CreateDb()
        {
            if (dbCreated)
                return;

            using var connection = PostgreSqlUtil.CreatePostgresDbConnection(settings.TemplateConnectionString);
            connection.Execute($@"CREATE DATABASE ""{TestDbName}"" TEMPLATE ""{templateName}""");

            dbCreated = true;
        }
    }

    /// <inheritdoc />
    void ITestDatabaseProvider.DropDatabase(DbContext dbContext) => DropDatabase();

    /// <inheritdoc cref="ITestDatabaseProvider.DropDatabase(DbContext)"/>
    public void DropDatabase()
    {
        if (!dbCreated)
            return;

        PostgreSqlUtil.DropDb(settings.TemplateConnectionString, TestDbName);
    }
}