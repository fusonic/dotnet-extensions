// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseNpgsqlDatabasePerTest(this DbContextOptionsBuilder builder, NpgsqlDatabasePerTestStore testStore, Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
        => builder.UseNpgsql(testStore.ConnectionString, npgsqlOptionsAction)
                  .AddInterceptors(new ConnectionOpeningInterceptor(testStore.CreateDatabase));
}