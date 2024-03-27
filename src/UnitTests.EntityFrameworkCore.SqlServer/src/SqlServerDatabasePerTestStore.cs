// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Data.SqlClient;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer;

public class SqlServerDatabasePerTestStore : ITestStore
{
    private readonly SqlServerDatabasePerTestStoreOptions options;
    private readonly SqlConnectionStringBuilder connectionStringBuilder;

    private readonly string masterConnectionString;

    public string ConnectionString => connectionStringBuilder.ConnectionString;

    private bool isDbCreated;

    public SqlServerDatabasePerTestStore(SqlServerDatabasePerTestStoreOptions options)
    {
        this.options = new SqlServerDatabasePerTestStoreOptions(options);

        connectionStringBuilder = new SqlConnectionStringBuilder(options.ConnectionString);
        masterConnectionString = connectionStringBuilder.ConnectionString;

        OnTestConstruction();
    }

    public void OnTestConstruction()
    {
        connectionStringBuilder.InitialCatalog = string.Concat(options.DatabasePrefix, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('='));
        isDbCreated = false;
    }

    public async Task OnTestEnd()
    {
        if (!isDbCreated)
            return;

        var connection = new SqlConnection(masterConnectionString);
        await using var _ = connection.ConfigureAwait(continueOnCapturedContext: false);
        await connection.OpenAsync().ConfigureAwait(false);

        var cmd = connection.CreateCommand();
        var dbName = connectionStringBuilder.InitialCatalog;

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

    public async Task CreateDatabase()
    {
        if (isDbCreated)
            return;

        await options.CreateDatabase(ConnectionString).ConfigureAwait(false);
        isDbCreated = true;
    }
}