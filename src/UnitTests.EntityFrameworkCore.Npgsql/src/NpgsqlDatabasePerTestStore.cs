// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Npgsql;
using Polly;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

public class NpgsqlDatabasePerTestStore : ITestStore
{
    private readonly NpgsqlDatabasePerTestStoreOptions options;
    private readonly NpgsqlConnectionStringBuilder connectionStringBuilder;

    public string ConnectionString => connectionStringBuilder.ConnectionString;

    private bool isDbCreated;

    public NpgsqlDatabasePerTestStore(NpgsqlDatabasePerTestStoreOptions options)
    {
        this.options = new NpgsqlDatabasePerTestStoreOptions(options);
        connectionStringBuilder = new NpgsqlConnectionStringBuilder(options.ConnectionString);

        ValidateConnectionString();
        OnTestConstruction();
    }

    private void ValidateConnectionString()
    {
        var templateDatabaseName = connectionStringBuilder.Database
                                ?? throw new ArgumentException("Missing database in connection string.");

        if (templateDatabaseName == "postgres")
            throw new ArgumentException("Connection string cannot use postgres as database. It should provide the name of the template database, even if it does not exist.");
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

        if (options.TemplateCreator != null)
        {
            await PostgreSqlUtil
                 .EnsureTemplateDbCreated(
                      options.ConnectionString,
                      options.TemplateCreator,
                      options.AlwaysCreateTemplate)
                 .ConfigureAwait(false);
        }

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

            await PostgreSqlUtil.CreateDatabase(options.ConnectionString, connectionStringBuilder.Database!).ConfigureAwait(false);

            isDbCreated = true;
        }
    }
}