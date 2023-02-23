// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer;
public class SqlServerDatabasePerTestStoreOptions
{
    public SqlServerDatabasePerTestStoreOptions()
    { }

    [SetsRequiredMembers]
    internal SqlServerDatabasePerTestStoreOptions(SqlServerDatabasePerTestStoreOptions copyFrom)
    {
        ConnectionString = copyFrom.ConnectionString;
        CreateDatabase = copyFrom.CreateDatabase;
    }

    /// <summary> Connection string to the master database. </summary>
    public required string ConnectionString { get; set; }

    /// <summary> Action to create the database on first connect. </summary>
    public required Func<string, Task> CreateDatabase { get; set; }
}