using Fusonic.Extensions.UnitTests;

namespace Example.Lib.Tests;

public class TestBase : TestBase<TestFixture>
{
    public TestBase(TestFixture fixture) : base(fixture)
    { }
}

public class TestBase<TFixture> : UnitTest<TFixture>
    where TFixture : TestFixture
{
    public TestBase(TFixture fixture) : base(fixture)
    { }
}