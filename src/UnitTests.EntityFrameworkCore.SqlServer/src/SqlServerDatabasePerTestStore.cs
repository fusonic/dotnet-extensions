// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer;

public class SqlServerDatabasePerTestStore : ITestStore
{
   
    private readonly SqlConnectionStringBuilder connectionStringBuilder;

    public string TemplateConnectionString { get; }
    public string DataDirectoryPath { get; }
    public string? DatabasePrefix { get; }

    public string ConnectionString => connectionStringBuilder.ConnectionString;

    private bool isDbCreated;

    /*    /// <summary> Connection string to the template database. </summary>
    public required string ConnectionString { get; set; }
    
    /// <summary>
    /// Path to the directory where the database files are stored. Defaults to the default SQL Server data directory in the docker image /var/opt/mssql/data.
    /// </summary>
    public string DataDirectoryPath
    {
        get => dataDirectoryPath;
        set => dataDirectoryPath = value.TrimEnd('/').TrimEnd('\\');
    }

    // <summary> Allows defining an optional prefix for the randomly generated test database names. </summary>
    public string? DatabasePrefix { get; set; }*/

    /// <summary>
    /// Initializes a new instance of the SqlServerDatabasePerTestStore class, configuring a per-test SQL Server database.
    /// </summary>
    /// <remarks>This constructor sets up the store to create isolated SQL Server databases for each test,
    /// based on the provided template connection string. Ensure that the template connection string points to a valid
    /// and accessible database.</remarks>
    /// <param name="templateConnectionString">The connection string to the template database used as the basis for creating per-test databases.</param>
    /// <param name="dataDirectoryPath">The path to the directory where database files will be stored. Defaults to "/var/opt/mssql/data".</param>
    /// <param name="databasePrefix">An optional prefix to use for the randomly generated test database names.</param>
    public SqlServerDatabasePerTestStore(string templateConnectionString, string dataDirectoryPath = "/var/opt/mssql/data", string? databasePrefix = null)
    {
        connectionStringBuilder = new SqlConnectionStringBuilder(templateConnectionString);
        TemplateConnectionString = templateConnectionString;
        DataDirectoryPath = dataDirectoryPath;
        DatabasePrefix = databasePrefix;

        var templateCatalogName = connectionStringBuilder.InitialCatalog
                               ?? throw new ArgumentException("Missing initial catalog in connection string.", nameof(templateConnectionString));

        if (templateCatalogName == "master")
            throw new ArgumentException("Connection string cannot use master as initial catalog. It should provide the name of the template catalog, even if it does not exist.", nameof(templateConnectionString));

        OnTestConstruction();
    }

    public void OnTestConstruction()
    {
        var dbName = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('/', '_');
        connectionStringBuilder.InitialCatalog = $"{DatabasePrefix}{dbName}";
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

        await SqlServerTestUtil.CreateDatabase(
            TemplateConnectionString, 
            connectionStringBuilder.InitialCatalog, 
            DataDirectoryPath).ConfigureAwait(false);

        isDbCreated = true;
    }
}