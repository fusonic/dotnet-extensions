using System.Reflection;
using Fusonic.Extensions.UnitTests.XunitExtensibility;
using Xunit;

namespace Fusonic.Extensions.UnitTests.Tests
{
    public class BeforeAfterTestInvokeAttributeTests : TestBase
    {
        public BeforeAfterTestInvokeAttributeTests(TestFixture fixture) : base(fixture)
        { }

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
}