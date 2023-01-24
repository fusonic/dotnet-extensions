// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;

public abstract class TestBase : TestBase<TestFixture>
{
    protected TestBase(TestFixture fixture) : base(fixture)
    { }
}

public abstract class TestBase<TFixture> : DatabaseUnitTest<TestDbContext, TFixture>
    where TFixture : TestFixture
{
    protected TestBase(TFixture fixture) : base(fixture)
    { }
}
