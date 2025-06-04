// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore;

public abstract class DatabaseUnitTest<TDbContext, TFixture>(TFixture fixture) : DatabaseUnitTest<TFixture>(fixture)
    where TDbContext : DbContext
    where TFixture : class, IDependencyInjectionTestFixture
{
    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected void Query(Action<TDbContext> query) => base.Query(query);

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected TResult Query<TResult>(Func<TDbContext, TResult> query) => base.Query(query);

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected Task QueryAsync(Func<TDbContext, Task> query) => base.QueryAsync(query);

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected Task<TResult> QueryAsync<TResult>(Func<TDbContext, Task<TResult>> query) => base.QueryAsync(query);
}