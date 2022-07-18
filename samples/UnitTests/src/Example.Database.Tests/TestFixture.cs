using Example.Database.Data;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Fusonic.Extensions.UnitTests.Adapters.InMemoryDatabase;
using Fusonic.Extensions.UnitTests.Adapters.PostgreSql;
using Microsoft.Extensions.Configuration;
using SimpleInjector;

namespace Example.Database.Tests;

public class TestFixture : DatabaseFixture<AppDbContext>
{
    public TestSettings TestSettings { get; } = new();

    protected override IConfiguration BuildConfiguration()
    {
        var configuration = base.BuildConfiguration();
        configuration.Bind(TestSettings);
        return configuration;
    }

    protected override void ConfigureDatabaseProviders(DatabaseFixtureConfiguration<AppDbContext> configuration)
    {
        // With a postgres having an InMemory data volume you usually don't want this. It's only for demonstration purposes.
        configuration.UsePostgreSqlDatabase(TestSettings.ConnectionString, TestSettings.TestDbPrefix, TestSettings.TestDbTemplate)
                     .UseInMemoryDatabase(seed: ctx => new TestDataSeed(ctx).Seed())
                     .UseDefaultProviderAttribute(new InMemoryTestAttribute());
 
        if (bool.TryParse(Environment.GetEnvironmentVariable("NIGHTLY"), out var isNightly) && isNightly)
            configuration.UseProviderAttributeReplacer(_ => new PostgreSqlTestAttribute());

        // With a postgres having an InMemory data volume you usually would have this configuration:
        // configuration.UsePostgreSqlDatabase(TestSettings.ConnectionString, TestSettings.TestDbPrefix, TestSettings.TestDbTemplate)
        //              .UseDefaultProviderAttribute(new PostgreSqlTestAttribute());
    }

    protected sealed override void RegisterCoreDependencies(Container container)
    {
        base.RegisterCoreDependencies(container);
        container.Register<PersonService>();
    }
}