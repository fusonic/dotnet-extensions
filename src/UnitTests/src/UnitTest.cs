// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Fusonic.Extensions.UnitTests.SimpleInjector;
using MediatR;
using SimpleInjector;
using Xunit;

namespace Fusonic.Extensions.UnitTests;

public abstract class UnitTest<TFixture> : IDisposable, IClassFixture<TFixture>
    where TFixture : UnitTestFixture
{
    private Scope currentScope;

    protected TFixture Fixture { get; }
    protected Container Container => currentScope.Container!;

    protected UnitTest(TFixture fixture)
    {
        Fixture = fixture;
        currentScope = fixture.BeginLifetimeScope();
    }

    /// <summary> Shortcut for Container.GetInstance{T} </summary>
    [DebuggerStepThrough]
    protected T GetInstance<T>()
        where T : class
        => Container.GetInstance<T>();

    /// <summary> Runs a mediator command in its own scope. Used to reduce possible side effects from test data creation and the like. </summary>
    [DebuggerStepThrough]
    protected Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => ScopedAsync(() => Container.GetInstance<IMediator>().Send(request));

    /// <summary> <see cref="ScopedAsync{TResult}"/> </summary>
    [DebuggerStepThrough]
    protected TResult Scoped<TResult>(Func<TResult> action)
    {
        var prevScope = currentScope;
        try
        {
            using var newScope = Fixture.BeginLifetimeScope();
            currentScope = newScope;
            return action();
        }
        finally
        {
            currentScope = prevScope;
        }
    }

    /// <summary> Runs an action in its own scope. Used to reduce possible side effects from test data creation and the like. </summary>
    [DebuggerStepThrough]
    protected async Task<TResult> ScopedAsync<TResult>(Func<Task<TResult>> taskFactory)
    {
        var prevScope = currentScope;
        try
        {
            await using var newScope = Fixture.BeginLifetimeScope();
            currentScope = newScope;
            return await taskFactory();
        }
        finally
        {
            currentScope = prevScope;
        }
    }

    /// <summary> <see cref="ScopedAsync{TResult}"/> </summary>
    [DebuggerStepThrough]
    protected void Scoped(Action action) => Scoped(() =>
    {
        action();
        return true;
    });

    /// <summary> <see cref="ScopedAsync{TResult}"/> </summary>
    [DebuggerStepThrough]
    protected Task ScopedAsync(Func<Task> taskFactory) => ScopedAsync(async () =>
    {
        await taskFactory();
        return true;
    });

    public virtual void Dispose()
    {
        currentScope?.Dispose();
        TestScopedLifestyle.CleanupTestScopes();
        GC.SuppressFinalize(this);
    }
}
