// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseSqlServerDatabasePerTest(this DbContextOptionsBuilder builder, SqlServerDatabasePerTestStore testStore, Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        => builder.UseSqlServer(testStore.ConnectionString, sqlServerOptionsAction)
                  .AddInterceptors(new ConnectionOpeningInterceptor(testStore.CreateDatabase));
}