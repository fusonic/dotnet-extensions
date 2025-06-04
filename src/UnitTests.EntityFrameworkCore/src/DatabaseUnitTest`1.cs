// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore;

public abstract class DatabaseUnitTest<TFixture> : DependencyInjectionUnitTest<TFixture>, IAsyncLifetime
    where TFixture : class, IDependencyInjectionTestFixture
{
    protected DatabaseUnitTest(TFixture fixture) : base(fixture) => GetInstance<ITestStore>().OnTestConstruction();

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected void Query<TDbContext>(Action<TDbContext> query)
        where TDbContext : DbContext
        => Query<TDbContext, bool>(ctx =>
        {
            query(ctx);
            return true;
        });

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected TResult Query<TDbContext, TResult>(Func<TDbContext, TResult> query)
        where TDbContext : DbContext
    {
        var resultType = typeof(TResult);
        if (resultType.IsGenericType)
            resultType = resultType.GetGenericTypeDefinition();

        return resultType == typeof(Task) || resultType == typeof(Task<>) || resultType == typeof(ValueTask) || resultType == typeof(ValueTask<>)
            ? throw new InvalidOperationException("This is the wrong method for async queries. Use QueryAsync() instead.")
            : Scoped(() =>
            {
                using var dbContext = GetInstance<TDbContext>();
                return query(dbContext);
            });
    }

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected Task QueryAsync<TDbContext>(Func<TDbContext, Task> query)
        where TDbContext : DbContext
        => QueryAsync<TDbContext, bool>(async ctx =>
        {
            await query(ctx);
            return true;
        });

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected Task<TResult> QueryAsync<TDbContext, TResult>(Func<TDbContext, Task<TResult>> query)
        where TDbContext : DbContext
        => ScopedAsync(async () =>
        {
            await using var dbContext = GetInstance<TDbContext>();
            return await query(dbContext);
        });

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;
    public virtual async ValueTask DisposeAsync()
    {
        await GetInstance<ITestStore>().OnTestEnd().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}