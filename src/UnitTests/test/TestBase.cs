// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests.Tests
{
    public class TestBase : UnitTest<TestFixture>
    {
        public TestBase(TestFixture fixture) : base(fixture)
        { }
    }
}