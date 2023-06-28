// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests;
using Fusonic.Extensions.UnitTests.SimpleInjector;

namespace Fusonic.Extensions.AspNetCore.Tests;

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
}
