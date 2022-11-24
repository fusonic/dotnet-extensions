// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;
using Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;

// TODO: Remove with .NET 7.0.1, false positive fixed in https://github.com/dotnet/roslyn-analyzers/pull/6278
#pragma warning disable CA1852

// Example:
// dotnet run --project ./src/UnitTests.EntityFrameworkCore.Npgsql/template/UnitTests.EntityFrameworkCore.Npgsql.TestDbTemplateCreator.csproj "Host=127.0.0.1;Port=5433;Database=fusonic_extensions_test;Username=postgres;Password=developer"

if (args.Length == 0)
{
    Console.Out.WriteLine("Missing connection string.");
    return 1;
}

var connectionString = args[0];

PostgreSqlUtil.CreateTestDbTemplate<TestDbContext>(connectionString, o => new TestDbContext(o));

return 0;
