// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Fusonic.Extensions.UnitTests;
using Fusonic.Extensions.UnitTests.SimpleInjector;

namespace Fusonic.Extensions.Mediator.Tests;

public abstract class TestBase(TestFixture fixture) : TestBase<TestFixture>(fixture);

public abstract class TestBase<TFixture>(TFixture fixture) : DependencyInjectionUnitTest<TFixture>(fixture)
    where TFixture : SimpleInjectorTestFixture
{
    [DebuggerStepThrough]
    protected Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => ScopedAsync(() => GetInstance<IMediator>().Send(request));

    [DebuggerStepThrough]
    protected Task PublishAsync<TNotification>(TNotification notification) where TNotification : INotification
        => ScopedAsync(() => GetInstance<IMediator>().Publish(notification));

    [DebuggerStepThrough]
    protected IAsyncEnumerable<TResponse> CreateAsyncEnumerable<TResponse>(IAsyncEnumerableRequest<TResponse> request)
    {
        return Scoped(() => GetInstance<IMediator>().CreateAsyncEnumerable(request));
    }
}