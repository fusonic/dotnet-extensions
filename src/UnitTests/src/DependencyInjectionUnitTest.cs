// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Xunit;

namespace Fusonic.Extensions.UnitTests;

public abstract class DependencyInjectionUnitTest<TFixture>(TFixture fixture) : IDisposable, IClassFixture<TFixture>
    where TFixture : class, IDependencyInjectionTestFixture
{
    private object currentScope = fixture.BeginScope();
    protected TFixture Fixture { get; } = fixture;

    /// <summary> Gets an instance of the requested service. </summary>
    [DebuggerStepThrough]
    protected T GetInstance<T>()
        where T : class => Fixture.GetInstance<T>(currentScope);

    /// <summary> Gets an instance of the requested service type. </summary>
    [DebuggerStepThrough]
    protected object GetInstance(Type serviceType) => Fixture.GetInstance(currentScope, serviceType);

    /// <summary> <see cref="ScopedAsync{TResult}"/> </summary>
    [DebuggerStepThrough]
    protected TResult Scoped<TResult>(Func<TResult> action)
    {
        var prevScope = currentScope;
        try
        {
            var newScope = Fixture.BeginScope();
            currentScope = newScope;
            try
            {
                return action();
            }
            finally
            {
                if (newScope is IDisposable d)
                    d.Dispose();
            }
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
            var newScope = Fixture.BeginScope();
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
        (currentScope as IDisposable)?.Dispose();
        GC.SuppressFinalize(this);
    }
}