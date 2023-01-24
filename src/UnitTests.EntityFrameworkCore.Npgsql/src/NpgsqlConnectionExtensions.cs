// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Npgsql;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

internal static class NpgsqlConnectionExtensions
{
    public static async Task ExecuteAsync(this NpgsqlConnection connection, string sql)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }
}