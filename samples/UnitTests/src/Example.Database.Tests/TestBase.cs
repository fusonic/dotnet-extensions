using Example.Database.Data;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

namespace Example.Database.Tests;

public class TestBase : TestBase<TestFixture>
{
    public TestBase(TestFixture fixture) : base(fixture)
    { }
}

public class TestBase<TFixture> : DatabaseUnitTest<AppDbContext, TFixture>
    where TFixture : TestFixture
{
    public TestBase(TFixture fixture) : base(fixture)
    { }
}