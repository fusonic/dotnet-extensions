// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Data.SqlClient;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer;

public class SqlServerDatabasePerTestStore : ITestStore
{
    private readonly SqlServerDatabasePerTestStoreOptions options;
    private readonly SqlConnectionStringBuilder connectionStringBuilder;

    public string ConnectionString => connectionStringBuilder.ConnectionString;

    private bool isDbCreated;

    public SqlServerDatabasePerTestStore(SqlServerDatabasePerTestStoreOptions options)
    {
        this.options = new SqlServerDatabasePerTestStoreOptions(options);
        connectionStringBuilder = new SqlConnectionStringBuilder(options.ConnectionString);
        
        ValidateConnectionString();
        OnTestConstruction();
    }

    private void ValidateConnectionString()
    {
        var templateCatalogName = connectionStringBuilder.InitialCatalog
                               ?? throw new ArgumentException("Missing initial catalog in connection string.");

        if (templateCatalogName == "master")
            throw new ArgumentException("Connection string cannot use master as initial catalog. It should provide the name of the template catalog, even if it does not exist.");
    }

    public void OnTestConstruction()
    {
        var dbName = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('/', '_');
        connectionStringBuilder.InitialCatalog = $"{options.DatabasePrefix}{dbName}";
        isDbCreated = false;
    }

    public async Task OnTestEnd()
    {
        if (isDbCreated)
            await SqlServerTestUtil.DropDatabase(ConnectionString).ConfigureAwait(false);
    }

    public async Task CreateDatabase()
    {
        if (isDbCreated)
            return;

        if (options.TemplateCreator != null)
        {
            await SqlServerTestUtil
                 .EnsureTemplateDbCreated(
                      options.ConnectionString,
                      options.TemplateCreator,
                      options.AlwaysCreateTemplate)
                 .ConfigureAwait(false);
        }

        await SqlServerTestUtil.CreateDatabase(
            options.ConnectionString, 
            connectionStringBuilder.InitialCatalog, 
            options.DataDirectoryPath).ConfigureAwait(false);

        isDbCreated = true;
    }
}