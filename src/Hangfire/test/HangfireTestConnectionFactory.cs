// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Hangfire.PostgreSql;
using Npgsql;

namespace Fusonic.Extensions.Hangfire.Tests;

public class HangfireTestConnectionFactory : IConnectionFactory
{
    private readonly Func<string> getConnectionString;
    public HangfireTestConnectionFactory(Func<string> getConnectionString) => this.getConnectionString = getConnectionString;

    public NpgsqlConnection GetOrCreateConnection() => new(getConnectionString());
}