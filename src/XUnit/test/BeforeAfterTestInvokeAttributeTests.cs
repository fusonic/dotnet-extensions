// // Copyright (c) Fusonic GmbH. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Fusonic.Extensions.XUnit.Framework;
using Xunit;

namespace Fusonic.Extensions.XUnit.Tests;

public class BeforeAfterTestInvokeAttributeTests
{
    [Test]
    [Fact]
    public void AttributeCalled()
    {
        Assert.True(TestAttribute.BeforeCalled);
        Assert.False(TestAttribute.AfterCalled); //gets called after the test...
    }

    public class TestAttribute : BeforeAfterTestInvokeAttribute
    {
        public static bool BeforeCalled { get; set; }
        public static bool AfterCalled { get; set; }

        public override void Before(MethodInfo methodUnderTest) => BeforeCalled = true;
        public override void After(MethodInfo methodUnderTest) => AfterCalled = true;
    }
}