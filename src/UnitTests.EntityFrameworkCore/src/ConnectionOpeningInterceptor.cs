// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore;

/// <summary>
/// This interceptor intercepts the connection opening event and executes the action defined in the ctor.
/// </summary>
public sealed class ConnectionOpeningInterceptor : DbConnectionInterceptor
{
    private readonly Func<Task> onOpening;

    public ConnectionOpeningInterceptor(Func<Task> onOpening) => this.onOpening = onOpening;

    public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
    {
        // Need to force onOpening to run outside of XUnits synchronization context to avoid deadlocks (XUnit thread pool may be exhausted already)
        Task.Run(onOpening).ConfigureAwait(false).GetAwaiter().GetResult();
        return base.ConnectionOpening(connection, eventData, result);
    }

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = new())
    {
        // Need to force onOpening to run outside of XUnits synchronization context to avoid deadlocks (XUnit thread pool may be exhausted already)
        await onOpening().ConfigureAwait(false);
        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }
}