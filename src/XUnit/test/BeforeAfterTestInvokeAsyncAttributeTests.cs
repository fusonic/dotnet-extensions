using System.Reflection;
using Fusonic.Extensions.XUnit.Framework;
using Xunit;

namespace Fusonic.Extensions.XUnit.Tests;

public class BeforeAfterTestInvokeAsyncAttributeTests
{
    [Fact]
    [TestAsync]
    public void AsyncAttributeCalled()
    {
        Assert.True(TestAsyncAttribute.BeforeCalled);
        Assert.False(TestAsyncAttribute.AfterCalled); //gets called after the test...
    }

    public class TestAsyncAttribute : BeforeAfterTestInvokeAsyncAttribute
    {
        public static bool BeforeCalled { get; set; }
        public static bool AfterCalled { get; set; }

        public override Task BeforeAsync(MethodInfo methodUnderTest)
        {
            BeforeCalled = true;
            return Task.CompletedTask;
        }

        public override Task AfterAsync(MethodInfo methodUnderTest)
        {
            AfterCalled = true;
            return Task.CompletedTask;
        }
    }
}