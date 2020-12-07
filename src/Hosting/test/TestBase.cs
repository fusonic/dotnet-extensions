// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests;

namespace Fusonic.Extensions.Hosting.Tests
{
    public class TestBase<TFixture> : UnitTest<TFixture>
        where TFixture : TestFixture
    {
        public TestBase(TFixture fixture) : base(fixture)
        { }
    }

    public class TestBase : TestBase<TestFixture>
    {
        public TestBase(TestFixture fixture) : base(fixture)
        { }
    }
}