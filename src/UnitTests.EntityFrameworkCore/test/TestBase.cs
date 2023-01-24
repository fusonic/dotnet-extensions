// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests;

public abstract class TestBase : DatabaseUnitTest<TestDbContext, TestFixture>
{
    protected TestBase(TestFixture fixture) : base(fixture) { }
}