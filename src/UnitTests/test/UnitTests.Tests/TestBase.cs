namespace Fusonic.Extensions.UnitTests.Tests
{
    public class TestBase : UnitTest<TestFixture>
    {
        public TestBase(TestFixture fixture) : base(fixture)
        { }
    }
}