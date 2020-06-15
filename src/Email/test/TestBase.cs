using Fusonic.Extensions.UnitTests;

namespace Fusonic.Extensions.Email.Tests
{
    public abstract class TestBase : TestBase<TestFixture>
    {
        protected TestBase(TestFixture fixture) : base(fixture)
        { }
    }

    public abstract class TestBase<TFixture> : UnitTest<TFixture>
        where TFixture : TestFixture
    {
        protected TestBase(TFixture fixture) : base(fixture)
        { }
    }
}