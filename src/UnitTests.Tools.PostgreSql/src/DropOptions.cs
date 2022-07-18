// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using CommandLine;
using Fusonic.Extensions.UnitTests.Adapters.PostgreSql;

namespace Fusonic.Extensions.UnitTests.Tools.PostgreSql;

[Verb("drop", HelpText = "Drops a specific database. All connected users will be terminated before.")]
public class DropOptions
{
    [Option('c', "connectionstring", Required = true, HelpText = "Connection string to the database that should be dropped. Terminates all connected sessions, so dropping should always succeed (given the proper permissions).")]
    public string? ConnectionString { get; set; }

    [Option('d', "database", HelpText = "Drop a different database than the DB given in the connection string.")]
    public string? Database { get; set; }

    public int Execute()
    {
        string? dbName;
        if (!string.IsNullOrWhiteSpace(Database))
        {
            dbName = Database;
        }
        else
        {
            dbName = PostgreSqlUtil.GetDatabaseName(ConnectionString!);
            if (dbName == null)
            {
                Console.Error.WriteLine("Could not find database name in connection string.");
                return 1;
            }
        }

        PostgreSqlUtil.DropDb(ConnectionString!, dbName!);
        return 0;
    }
}
