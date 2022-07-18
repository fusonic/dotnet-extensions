// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

#pragma warning disable CS0618 // Type or member is obsolete
[LimitTestConcurrency] // Disabled by default now. Only enabled if SetMaxTestConcurrency is explicitly set. Will be removed with 7.0.
#pragma warning restore CS0618 // Type or member is obsolete
public abstract class DatabaseUnitTest<TDbContext, TFixture> : UnitTest<TFixture>
    where TDbContext : DbContext
    where TFixture : DatabaseFixture<TDbContext>
{
    protected DatabaseUnitTest(TFixture fixture) : base(fixture) => Query(dbContext =>
    {
        var provider = GetInstance<ITestDatabaseProvider<TDbContext>>();
        provider.SeedDatabase(dbContext);
    });

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected void Query(Action<TDbContext> query) => Query(ctx =>
    {
        query(ctx);
        return true;
    });

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected TResult Query<TResult>(Func<TDbContext, TResult> query)
    {
        var resultType = typeof(TResult);
        if (resultType.IsGenericType)
            resultType = resultType.GetGenericTypeDefinition();

        if (resultType == typeof(Task) || resultType == typeof(Task<>) || resultType == typeof(ValueTask) || resultType == typeof(ValueTask<>))
            throw new ArgumentException("This is the wrong method for async queries. Use QueryAsync() instead.");

        return Scoped(() =>
        {
            using var dbContext = GetInstance<TDbContext>();
            return query(dbContext);
        });
    }

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected Task QueryAsync(Func<TDbContext, Task> query) => QueryAsync(async ctx =>
    {
        await query(ctx);
        return true;
    });

    /// <summary> Executes a query in an own scope. </summary>
    [DebuggerStepThrough]
    protected Task<TResult> QueryAsync<TResult>(Func<TDbContext, Task<TResult>> query) => ScopedAsync(async () =>
    {
        await using var dbContext = GetInstance<TDbContext>();
        return await query(dbContext);
    });

    /// <inheritdoc cref="ITestDatabaseProvider{TDbContext}.CreateDatabase" />
    protected void CreateDatabase() => Query(dbContext =>
    {
        var provider = GetInstance<ITestDatabaseProvider<TDbContext>>();
        provider.CreateDatabase(dbContext);
    });

    public override void Dispose()
    {
        Query(dbContext =>
        {
            var provider = GetInstance<ITestDatabaseProvider<TDbContext>>();
            provider.DropDatabase(dbContext);
        });

        GC.SuppressFinalize(this);
        base.Dispose();
    }
}
