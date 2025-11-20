// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Npgsql;
using Polly;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

public class NpgsqlDatabasePerTestStore : ITestStore
{
    private readonly NpgsqlConnectionStringBuilder connectionStringBuilder;

    public string TemplateConnectionString { get; }
    public string ConnectionString => connectionStringBuilder.ConnectionString;

    private bool isDbCreated;

    /// <summary>
    /// Initializes a new instance of the NpgsqlDatabasePerTestStore.
    /// </summary>
    /// <remarks>This constructor sets up the store to create isolated PostgreSQL databases for each test,
    /// based on the provided template connection string. Ensure that the template connection string points to a valid
    /// and accessible database.</remarks>
    /// <param name="templateConnectionString">The connection string to use as a template for creating per-test database instances.</param>
    public NpgsqlDatabasePerTestStore(string templateConnectionString)
    {
        TemplateConnectionString = templateConnectionString;
        connectionStringBuilder = new NpgsqlConnectionStringBuilder(templateConnectionString);

        var templateDatabaseName = connectionStringBuilder.Database
                                ?? throw new ArgumentException("Missing database in connection string.", nameof(templateConnectionString));

        if (templateDatabaseName == "postgres")
            throw new ArgumentException("Connection string cannot use postgres as database. It should provide the name of the template database, even if it does not exist.", nameof(templateConnectionString));

        OnTestConstruction();
    }

    public void OnTestConstruction()
    {
        connectionStringBuilder.Database = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=');
        isDbCreated = false;
    }

    public async Task OnTestEnd()
    {
        if (isDbCreated)
            await PostgreSqlUtil.DropDatabase(ConnectionString).ConfigureAwait(false);
    }

    public async Task CreateDatabase()
    {
        if (isDbCreated)
            return;

        // Creating a DB from a template can cause an exception when done in parallel.
        // The lock usually prevents this, however, we still encounter race conditions
        // where we just have to retry.
        // 55006: source database "test_template" is being accessed by other users
        await Policy.Handle<NpgsqlException>(e => e.SqlState == "55006")
                    .WaitAndRetryAsync(30, _ => TimeSpan.FromMilliseconds(500))
                    .ExecuteAsync(CreateDb)
                    .ConfigureAwait(false);

        async Task CreateDb()
        {
            if (isDbCreated)
                return;

            await PostgreSqlUtil.CreateDatabase(TemplateConnectionString, connectionStringBuilder.Database!).ConfigureAwait(false);

            isDbCreated = true;
        }
    }
}