// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Hangfire.PostgreSql;
using Npgsql;

namespace Fusonic.Extensions.Hangfire.Tests;

public class HangfireTestConnectionFactory(Func<string> getConnectionString) : IConnectionFactory
{
    public NpgsqlConnection GetOrCreateConnection() => new(getConnectionString());
}