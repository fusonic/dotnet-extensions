// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fusonic.Extensions.UnitTests.Adapters.PostgreSql;

/// <summary>
/// This interceptor creates a test database on first connection. This also works on pooled connections as the test databases
/// are unique per test and thus always require a new connection.
/// </summary>
internal sealed class CreateDatabaseInterceptor<TDbContext> : DbConnectionInterceptor
    where TDbContext : DbContext
{
    private readonly Action<TDbContext> createDb;
    private bool dbCreated;

    public CreateDatabaseInterceptor(Action<TDbContext> createDb) => this.createDb = createDb;

    public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
    {
        CreateDatabase(eventData.Context);
        return base.ConnectionOpening(connection, eventData, result);
    }

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = new())
    {
        CreateDatabase(eventData.Context);
        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }

    private void CreateDatabase(DbContext? dbContext)
    {
        if (dbCreated || dbContext == null)
            return;

        createDb((TDbContext)dbContext);
        dbCreated = true;
    }
}