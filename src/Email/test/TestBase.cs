// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Fusonic.Extensions.Mediator;
using Fusonic.Extensions.UnitTests;
using Fusonic.Extensions.UnitTests.SimpleInjector;

namespace Fusonic.Extensions.Email.Tests;

public abstract class TestBase : TestBase<TestFixture>
{
    protected TestBase(TestFixture fixture) : base(fixture)
    { }
}

public abstract class TestBase<TFixture> : DependencyInjectionUnitTest<TFixture>
    where TFixture : SimpleInjectorTestFixture
{
    protected TestBase(TFixture fixture) : base(fixture)
    { }

    /// <summary> Runs a mediator command in its own scope. Used to reduce possible side effects from test data creation and the like. </summary>
    [DebuggerStepThrough]
    protected Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => ScopedAsync(() => GetInstance<IMediator>().Send(request));
}
