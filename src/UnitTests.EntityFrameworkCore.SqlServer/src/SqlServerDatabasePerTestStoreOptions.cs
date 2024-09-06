// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer;
public class SqlServerDatabasePerTestStoreOptions
{
    private string dataDirectoryPath = "/var/opt/mssql/data";

    public SqlServerDatabasePerTestStoreOptions()
    { }

    [SetsRequiredMembers]
    internal SqlServerDatabasePerTestStoreOptions(SqlServerDatabasePerTestStoreOptions copyFrom)
    {
        ConnectionString = copyFrom.ConnectionString;
        TemplateCreator = copyFrom.TemplateCreator;
        AlwaysCreateTemplate = copyFrom.AlwaysCreateTemplate;
        DataDirectoryPath = copyFrom.DataDirectoryPath;
        DatabasePrefix = copyFrom.DatabasePrefix;
    }

    /// <summary> Connection string to the master database. </summary>
    public required string ConnectionString { get; set; }

    /// <summary> Action to create the database template on first connect. Only gets executed once. If AlwaysCreateTemplate is set to false, the action only gets executed if the template database does not exist. </summary>
    public required Func<string, Task>? TemplateCreator { get; set; }
    
    /// <summary> Ignores an existing template database and always recreates the template on the first run. Ignored, if TemplateCreator is null. </summary>
    public bool AlwaysCreateTemplate { get; set; }

    /// <summary>
    /// Path to the directory where the database files are stored. Defaults to the default SQL Server data directory in the docker image /var/opt/mssql/data.
    /// </summary>
    public string DataDirectoryPath
    {
        get => dataDirectoryPath;
        set => dataDirectoryPath = value.TrimEnd('/').TrimEnd('\\');
    }

    // <summary> Allows defining an optional prefix for the randomly generated test database names. </summary>
    public string? DatabasePrefix { get; set; }
}