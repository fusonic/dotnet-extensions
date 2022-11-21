using Example.Database.Domain;
using FluentAssertions;
using Fusonic.Extensions.UnitTests.Adapters.PostgreSql;

namespace Example.Database.Tests;

// Note: It is assumed that you have docker and the postgres service from the root directory of this example is running
//       You can launch it using one of the run_postgres scripts. It starts a PostgreSQL server on port 5433 having
//       the data folder mounted to an InMemory (tmpfs) volume.
public class PersonServiceTests : TestBase
{
    public PersonServiceTests(TestFixture fixture) : base(fixture)
    { }

    [Fact]
    [PostgreSqlTest]
    public async Task ThisTestAlwaysUsesAPostgresDatabase()
    {
        // Arrange
        await QueryAsync(async ctx =>
        {
            ctx.Add(new Person("Henry"));
            await ctx.SaveChangesAsync();
        });

        // Act
        var result = await ScopedAsync(() => GetInstance<PersonService>().GetPersons());

        // Assert
        result.Should().HaveCount(2) // second one is from the seed
              .And.Contain(p => p.Name == "Henry");
    }

    [Fact]
    public async Task ThisTestRunsInMemoryByDefault()
    {
        // Arrange
        await QueryAsync(async ctx =>
        {
            ctx.Add(new Person("Henry2"));
            await ctx.SaveChangesAsync();
        });

        // Act
        var result = await ScopedAsync(() => GetInstance<PersonService>().GetPersons());

        // Assert
        result.Should().HaveCount(2) // second one is from the seed
              .And.Contain(p => p.Name == "Henry2");
    }

    // This test is executed 1500 times. Each test runs against the PostgreSQL server and each test gets its own database.
    // Having the servers data directory mounted to an InMemory (tmpfs) volume, this should still be very fast.
    [Theory]
    [MemberData(nameof(ALotOfTestData))]
    [PostgreSqlTest]
    public async Task ALotOfDatabaseTests(int postfix)
    {
        // Arrange
        await QueryAsync(async ctx =>
        {
            ctx.Add(new Person("Henry " + postfix));
            await ctx.SaveChangesAsync();
        });

        // Act
        var result = await ScopedAsync(() => GetInstance<PersonService>().GetPersons());

        // Assert
        result.Should().HaveCount(2) // second one is from the seed
              .And.Contain(p => p.Name == "Henry " + postfix);
    }

    public static IEnumerable<object[]> ALotOfTestData => Enumerable.Range(1, 1500)
                                                                    .Select(i => new object[] { i });
}