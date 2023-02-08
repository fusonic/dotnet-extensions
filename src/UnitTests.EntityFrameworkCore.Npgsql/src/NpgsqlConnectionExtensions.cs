// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Npgsql;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

internal static class NpgsqlConnectionExtensions
{
    public static async Task ExecuteAsync(this NpgsqlConnection connection, string sql)
    {
        var cmd = connection.CreateCommand();
        await using var _ = cmd.ConfigureAwait(false);
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }
}