// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Fusonic.Extensions.UnitTests;

namespace Fusonic.Extensions.Mediator.Tests;

public abstract class TestBase<TFixture>(TFixture fixture) : DependencyInjectionUnitTest<TFixture>(fixture)
    where TFixture : class, IDependencyInjectionTestFixture
{
    [DebuggerStepThrough]
    protected Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => ScopedAsync(() => GetInstance<IMediator>().Send(request));

    [DebuggerStepThrough]
    protected Task PublishAsync<TNotification>(TNotification notification) where TNotification : INotification
        => ScopedAsync(() => GetInstance<IMediator>().Publish(notification));
 }