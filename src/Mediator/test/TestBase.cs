// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests;
using Fusonic.Extensions.UnitTests.SimpleInjector;

namespace Fusonic.Extensions.Mediator.Tests;

public abstract class TestBase(TestFixture fixture) : TestBase<TestFixture>(fixture);

public abstract class TestBase<TFixture>(TFixture fixture) : DependencyInjectionUnitTest<TFixture>(fixture)
    where TFixture : SimpleInjectorTestFixture
{

}