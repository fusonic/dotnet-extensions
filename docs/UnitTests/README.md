# Unit tests

- [Unit tests](#unit-tests)
  - [Introduction](#introduction)
  - [Setup](#setup)
  - [Test settings / Microsoft.Extensions.Configuration](#test-settings--microsoftextensionsconfiguration)
  - [Database setup](#database-setup)
    - [Test base](#test-base)
    - [PostgreSQL - Configure DbContext](#postgresql---configure-dbcontext)
    - [PostgreSQL - Template](#postgresql---template)
    - [PostgreSQL - TestContainers](#postgresql---testcontainers)
    - [Microsoft SQL Server - Configure DbContext](#microsoft-sql-server---configure-dbcontext)
    - [Microsoft SQL Server - TestContainers](#microsoft-sql-server---testcontainers)
    - [Configuring any other database](#configuring-any-other-database)
    - [Support mulitple databases in a test](#support-mulitple-databases-in-a-test)
  - [Running in GitLab](#running-in-gitlab)

## Introduction

The unit test framwork tries to provide the following features & design goals:

- **Simplicity:** The unit test base classes provide several helper methods targeted to support you using our currently used default architectural structures and libraries. To reduce reading and writing overhead (clutter) the methods tend have short, less descriptive names. For example, `Scoped` instead of `RunInSeparateLifetimeScope`, but they aren't that much and are easy to learn.
  - **Resolving types:** Can be done using `GetInstance<T>()`
  - **Scope separation:** In reality, creating data and consuming that data is not done in the same scope. In order to be able to see issues when using different scopes in the unit tests you can run your code in the provided `Scoped` and `ScopedAsync` methods. They run your code in separate lifetime scopes of the DI-Container. Within the tests you usually want to run your data preparations and your queries in different scopes.

- **Dependency injection:** The test base is layed out to be support dependency injection. Extensions for IServiceProvider and SimpleInjector are provided.

- **Database support:** The basic framework, `Fusonic.Extensions.UnitTests` does not come with any support for Databases, but we do provide support for fast and parallel database tests with `Fusonic.Extensions.UnitTests.EntityFrameworkCore`, currently specifically supporting PostgreSQL with `Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql`.

- **Configuration support:** Microsoft.Extensions.Configuration is supported out of a box with usable default settings.

## Setup

Currently SimpleInjector and Microsofts dependency injection are supported.

Create a `TestBase` and a `TestFixture` for the assembly.

The `TestFixture` is used for registering your depdendencies. Override `ServiceProviderTestFixture` or `SimpleInjectorTestFixture`, depending on the DI container you want to use. Note that when using SimpleInjector, you must reference it in your projects, as it does not come as a transient build dependency when using `Fusonic.Extensions.UnitTests`.

The `TestBase` is used for tying up the test base from the extensions and your test fixture.

The setup of the `TestBase` and the `TestFixture` is quick and easy. In `RegisterCoreDependencies` you register those dependencies that you usually need for your tests (Mediator, Services, ASP.Net services and the like). It should not be overwritten in your specific tests, so consider to make it `sealed`. For your test specific fixtures use `RegisterDependencies` instead.

```cs
public class TestFixture : SimpleInjectorTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        //Your core dependencies here.
    }
}
```

The test class is also abstract and requires a fixture as type parameter. Create the following base classes:

```cs
public abstract class TestBase<TFixture> : DependencyInjectionUnitTest<TFixture>
    where TFixture : TestFixture
{
    protected TestBase(TFixture fixture) : base(fixture)
    { }
}

public abstract class TestBase : TestBase<TestFixture>
{
    protected TestBase(TestFixture fixture) : base(fixture)
    { }

    // Also add common helper methods and properties to this class, eg.

    /// <summary> Runs a mediator command in its own scope. Used to reduce possible side effects from test data creation and the like. </summary>
    [DebuggerStepThrough]
    protected Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => ScopedAsync(() => GetInstance<IMediator>().Send(request));
}
```

That's it, you're good to go. Inherit your classes from the `TestBase` and get all the fancy features. If you need own fixtures, inherit them from `TestFixture` and your test class from `TestBase<TFixture>`. Register your test specific dependencies by overriding `RegisterDependencies`.

```cs
public class FixtureForASpecificTestClass : TestFixture
{
    protected override void RegisterDependencies(Container container)
    {
        //Your test specific fixtures here.
        //No need to call the base as it does nothing.
    }
}
```

## Test settings / Microsoft.Extensions.Configuration

That's the default configuration:

```cs
public static IConfigurationBuilder ConfigureTestDefault(this IConfigurationBuilder builder, string basePath, Assembly assembly)
        => builder.SetBasePath(basePath) // basePath = Directory.GetCurrentDirectory()
                  .AddJsonFile("testsettings.json", optional: true)
                  .AddUserSecrets(assembly, optional: true) // assembly = GetType().Assembly
                  .AddEnvironmentVariables();
```

If you want to, overwrite it in your `TestBase` by overwriting `BuildConfiguration`. You can access the configuration with the `Configuration` property. Example:

```cs
protected override void RegisterDependencies(Container container)
{
    var settings = Configuration.Get<TestSettings>();
    // ...
}
```

## Database setup

For database support you can use `Fusonic.Extensions.UnitTests.EntityFrameworkCore`, optionally with specific support for 
- PostgreSQL in `Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql`
- SQL Server in `Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer`

The basic idea behind those is that every test gets its own database copy. This enables parallel database testing and avoids any issues from tests affecting other tests.

### Test base

A test base for your database tests is available called `DatabaseUnitTest`. Type parameters are the test fixture optionally the `DbContext`. Example base classes in your test lib:
```cs
public abstract class TestBase(TestFixture fixture) : TestBase<TestFixture>(fixture);

public abstract class TestBase<TFixture>(TFixture fixture) : DatabaseUnitTest<AppDbContext, TFixture>(fixture)
    where TFixture : TestFixture;
```

The `DatabaseUnitTest` provides the methods `Query` and `QueryAsync`. This is basically shortcut to resolving the `DbContext` and using it. So instead of writing

```cs
var count = await ScopedAsync(() => 
{
    var context = GetInstance<DbContext>();
    return context.Documents.CountAsync();
});
```
 you can just use
 ```cs
 var count = await QueryAsync(ctx => ctx.Documents.CountAsync());
 ```

`QueryAsync` uses the `DbContext`-type provided in `DatabaseUnitTest<,>`.

If you have multiple `DbContextTypes` you can always run queries for the other types with `QueryAsync<TDbContext>` or `Query<TDbContext>`. You can also use the `DatabaseUnitTest<>` base class without `TDbContext` if you do not want to use a default `DbContext`-type.

### PostgreSQL - Configure DbContext

A `TestStore` is used for handling the test databases. For PostgreSQL, you can use the `NpgsqlDatabasePerTestStore`, which creates a separate database for each test, based on a database template. You just have to pass it the connection string to the template and register it as follows:

```cs
public class TestFixture : ServiceProviderTestFixture
{
    protected sealed override void RegisterCoreDependencies(ServiceCollection services)
    {
        var testStore = new NpgsqlDatabasePerTestStore(Configuration.GetConnectionString("Npgsql")); // or TestStartup.ConnectionString when using TestContainers
        services.AddSingleton<ITestStore>(testStore);

        services.AddDbContext<AppDbContext>(b => b.UseNpgsqlDatabasePerTest(testStore));
    }
}
```

When using `IDbContextFactory`, the factory must be registered with scoped lifetime, not with the default singleton lifetime.  
```cs
services.AddDbContext<AppDbContext>(b => b.UseNpgsqlDatabasePerTest(testStore), ServiceLifetime.Scoped);
```

### PostgreSQL - Template

When using the `NpgsqlDatabasePerTest` it is assumed that you use a prepared database template. This template should have all migrations applied and may contain some seeded data. Each test gets a copy of this template. With the `PostgreSqlUtil`, we provide an easy way to create such a template.

You can either create a small console application that creates the template, or do it directly once in the test startup.

A simple way to create that template is provided in `PostgreSqlUtil`. Example:

```cs
PostgreSqlUtil.CreateTestDbTemplate<AppDbContext>(templateConnectionString, o => new AppDbContext(o), seed: ctx => new TestDataSeed(ctx).Seed());
```

By default this only creates the template, if the database does not exist yet. This speeds up testing in local development when running tests multiple times. You can change this behaviour to always create a fresh template by setting the parameter `overwrite` to `true`.

### PostgreSQL - TestContainers

To get your test PostgreSQL server up and running, a simple solution is to use [TestContainers](https://testcontainers.com/) and start one during test startup. This ensures that the test configuration in your local development and in your CI pipelines is the same.

For an example how we use TestContainers see [UnitTests.EntityFrameworkCore.Npgsql.Tests.TestStartup](../../src/UnitTests.EntityFrameworkCore.Npgsql/test/TestStartup.cs) and [TestFixture](../../src/UnitTests.EntityFrameworkCore.Npgsql/test/TestFixture.cs)

### Microsoft SQL Server - Configure DbContext

A `TestStore` is used for handling the test databases. For Microsoft SQL Server, you can use the `SqlServerDatabasePerTestStore`, which creates a separate database for each test. You just have to pass it the connection string to the template and register it as follows:

```cs
public class TestFixture : ServiceProviderTestFixture
{
    protected sealed override void RegisterCoreDependencies(ServiceCollection services)
    {
        var testStore = new SqlServerDatabasePerTestStore(Configuration.GetConnectionString("SqlServer")); // or TestStartup.ConnectionString when using TestContainers
        services.AddSingleton<ITestStore>(testStore);

        services.AddDbContext<AppDbContext>(b => b.UseSqlServerDatabasePerTest(testStore));
    }
}
```

The connection string must have the `Intial catalog` set. It determines the name of the template database. All tests will use a copy of the template database.

When using `IDbContextFactory`, the factory must be registered with scoped lifetime, not with the default singleton lifetime.  
```cs
services.AddDbContext<AppDbContext>(b => b.UseSqlServerDatabasePerTest(testStore), ServiceLifetime.Scoped);
```

### Microsoft SQL Server - TestContainers

To get your test SQL Server server up and running, a simple solution is to use [TestContainers](https://testcontainers.com/) and start one during test startup. This ensures that the test configuration in your local development and in your CI pipelines is the same.

For an example how we use TestContainers see [UnitTests.EntityFrameworkCore.SqlServer.Tests.TestStartup](../../src/UnitTests.EntityFrameworkCore.SqlServer/test/TestStartup.cs) and [TestFixture](../../src/UnitTests.EntityFrameworkCore.SqlServer/test/TestFixture.cs)

### Configuring any other database

The database support is not limited to PostgreSql and SQL Server. You just have to implement and register the `ITestStore`.

For a simple example with SqLite, check [UnitTests.EntityFrameworkCore.Tests.SqliteTestStore](../../src/UnitTests.EntityFrameworkCore/test/SqliteTestStore.cs) and [TestFixture](../../src/UnitTests.EntityFrameworkCore/test/TestFixture.cs).

### Support mulitple databases in a test

You can test with multiple, different database systems at once. The setup stays basically the same, but instead of registering the test stores one by one, use the `AggregateTestStore`. Example:

```cs
public class TestFixture : ServiceProviderTestFixture
{
    private void RegisterDatabase(IServiceCollection services)
    {
        // Register Npgsql (PostgreSQL) using TestContainers in the TestStartup
        var npgsqlTestStore = new NpgsqlDatabasePerTestStore(TestStartup.NpgsqlConnectionString);
        services.AddDbContext<NpgsqlDbContext>(b => b.UseNpgsqlDatabasePerTest(npgsqlTestStore));

        // Register SQL Server using TestContainers in the TestStartup
        var sqlServerTestStore = new SqlServerDatabasePerTestStore(TestStartup.SqlServerConnectionString);
        services.AddDbContext<SqlServerDbContext>(b => b.UseSqlServerDatabasePerTest(sqlServerTestStore));

        // Combine the test stores in the AggregateTestStore
        services.AddSingleton<ITestStore>(new AggregateTestStore(npgsqlTestStore, sqlServerTestStore));
    }
}
```

## Running in GitLab

In order to use TestContainers in GitLab, start `docker:dind-rootless` as a service. When running rootless dind, you also must set `TESTCONTAINERS_RYUK_DISABLED` to `true`, as there is no `docker.sock` available. Ryuk is responsible for cleaning up the test containers, even when test jobs get cancelled. Disabling it should be safe though, as the containers started within `docker:dind` get cleaned up anyway after the job ends.

Example:
```yaml
variables:
  DOCKER_VERSION: "29"
  DOCKER_BUILDKIT: 1
  DOCKER_HOST: tcp://docker:2376
  DOCKER_TLS_CERTDIR: "/certs/${CI_JOB_ID}"
  DOCKER_TLS_VERIFY: 1
  DOCKER_CERT_PATH: "/certs/${CI_JOB_ID}/client"
  TESTCONTAINERS_RYUK_DISABLED: "true" # Disable Ryuk as we're running in dind-rootless and there is no docker.sock available.

dotnet:test:
  services:
    - docker:${DOCKER_VERSION}-dind-rootless
  script:
    - echo "Running Tests..."
```