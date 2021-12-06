// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Xunit;

namespace Fusonic.Extensions.UnitTests.Tests;

public class TestContextTests : TestBase
{
    public TestContextTests(TestFixture fixture) : base(fixture)
    { }

    [Fact]
    public void TestClassIsSet()
    {
        Assert.NotNull(TestContext.TestClass);
        Assert.Equal(typeof(TestContextTests), TestContext.TestClass);
    }

    [Fact]
    public void TestMethodIsSet()
    {
        Assert.NotNull(TestContext.TestMethod);
        Assert.Equal(nameof(TestMethodIsSet), TestContext.TestMethod.Name);
    }

    [Fact]
    public void TestOutputIsSet()
    {
        Assert.NotNull(TestContext.Out);
        TestContext.WriteLine("Yep. It works.");
        TestContext.WriteLine("{0}. It works {1}.", true, "too");
    }

    [Fact]
    public void CanStoreItems()
    {
        TestContext.Items["2"] = 3;

        Assert.Equal(3, TestContext.Items["2"]);
        Assert.Null(TestContext.Items["Something"]);
    }
}
