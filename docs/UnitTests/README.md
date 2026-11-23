# Unit tests

This documentation covers the following packages:

| Package | Purpose |
|---|---|
| `Fusonic.Extensions.UnitTests` | Test base classes with dependency injection (SimpleInjector or Microsoft.Extensions.DependencyInjection) and configuration |
| `Fusonic.Extensions.UnitTests.EntityFrameworkCore` | Database tests with EF Core: one database per test |
| `Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql` | PostgreSQL support |
| `Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer` | Microsoft SQL Server support |

The packages target xUnit v3. Your test project must reference the test framework itself (`xunit.v3`, a runner such as `xunit.runner.visualstudio` or Microsoft.Testing.Platform) and the assertion library of your choice. When using SimpleInjector, reference `SimpleInjector` as well. It is not a transitive dependency of `Fusonic.Extensions.UnitTests`.

- [Unit tests](#unit-tests)
  - [Introduction](#introduction)
  - [Setup](#setup)
    - [Microsoft.Extensions.DependencyInjection](#microsoftextensionsdependencyinjection)
    - [SimpleInjector](#simpleinjector)
    - [Test base](#test-base)
  - [Test settings / Microsoft.Extensions.Configuration](#test-settings--microsoftextensionsconfiguration)
  - [Database setup](#database-setup)
    - [Database test base](#database-test-base)
    - [Test startup](#test-startup)
    - [PostgreSQL](#postgresql)
    - [Microsoft SQL Server](#microsoft-sql-server)
    - [Database tests with SimpleInjector](#database-tests-with-simpleinjector)
    - [Configuring any other database](#configuring-any-other-database)
    - [Support multiple databases in a test](#support-multiple-databases-in-a-test)
  - [Running in GitLab](#running-in-gitlab)

## Introduction

The unit test framework provides the following features:

- **Simplicity:** The helper methods have short names to reduce clutter, e.g. `Scoped` instead of `RunInSeparateLifetimeScope`.
  - **Resolving types:** `GetInstance<T>()` or `GetInstance(Type)`.
  - **Scope separation:** In reality, creating data and consuming that data is not done in the same scope. `Scoped` and `ScopedAsync` run your code in a separate lifetime scope of the DI container, so you see issues that only occur across scopes. Run data preparation and queries in different scopes.
- **Dependency injection:** SimpleInjector and Microsoft.Extensions.DependencyInjection are supported.
- **Database support:** `Fusonic.Extensions.UnitTests.EntityFrameworkCore` enables fast, parallel database tests, with providers for PostgreSQL and Microsoft SQL Server.
- **Configuration support:** Microsoft.Extensions.Configuration is supported out of the box with usable default settings.

## Setup

Create a `TestFixture` to register your dependencies and a `TestBase` that ties the test base class of the extensions to your fixture. All types are in the namespace `Fusonic.Extensions.UnitTests`.

`DependencyInjectionUnitTest<TFixture>` implements xUnit's `IClassFixture<TFixture>`, so every test class gets one fixture instance and therefore one container.

In `RegisterCoreDependencies` you register the dependencies that all tests need (Mediator, services, ASP.NET Core services and the like). Seal it, so specific fixtures do not override it by accident. Use `RegisterDependencies` for test specific registrations.

### Microsoft.Extensions.DependencyInjection

Inherit from `ServiceProviderTestFixture`. The service provider is built with `ValidateOnBuild` and `ValidateScopes` enabled. Override `ServiceProviderOptions` to change that.

```cs
public class TestFixture : ServiceProviderTestFixture
{
    protected sealed override void RegisterCoreDependencies(IServiceCollection services)
    {
        // Your core dependencies here.
    }
}
```

### SimpleInjector

Inherit from `SimpleInjectorTestFixture`. The container is configured with `DefaultLifestyle = Lifestyle.Scoped`, `DefaultScopedLifestyle = new AsyncScopedLifestyle()` and `AllowOverridingRegistrations = true`, and it is verified after registration. Override `VerifyContainer` to return `false` to skip the verification.

```cs
public class TestFixture : SimpleInjectorTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        // Your core dependencies here.
    }
}
```

### Test base

The test class requires the fixture as type parameter. Create the following base classes:

```cs
public abstract class TestBase<TFixture>(TFixture fixture) : DependencyInjectionUnitTest<TFixture>(fixture)
    where TFixture : TestFixture;

public abstract class TestBase(TestFixture fixture) : TestBase<TestFixture>(fixture)
{
    // Add common helper methods and properties to this class, e.g.:

    /// <summary> Runs a mediator command in its own scope. Used to reduce possible side effects from test data creation and the like. </summary>
    [DebuggerStepThrough]
    protected Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => ScopedAsync(() => GetInstance<IMediator>().Send(request));
}
```

Inherit your test classes from `TestBase`. If you need specific fixtures, inherit them from `TestFixture`, override `RegisterDependencies` and inherit the test class from `TestBase<TFixture>`. The base implementation of `RegisterDependencies` is empty.

```cs
public class FixtureForASpecificTestClass : TestFixture
{
    protected override void RegisterDependencies(IServiceCollection services) // Container container for SimpleInjector
    {
        // Your test specific registrations here.
    }
}

public class SpecificTests(FixtureForASpecificTestClass fixture) : TestBase<FixtureForASpecificTestClass>(fixture)
{
    [Fact]
    public async Task Test()
    {
        var result = await ScopedAsync(() => GetInstance<IMyService>().DoAsync());
        // ...
    }
}
```

## Test settings / Microsoft.Extensions.Configuration

The fixture provides the configuration in its `Configuration` property (`Fixture.Configuration` in the test class). That's the default configuration:

```cs
builder.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("testsettings.json", optional: true)
       .AddUserSecrets(fixtureAssembly, optional: true)
       .AddEnvironmentVariables();
```

The default configuration is built once and shared by all fixtures of the test assembly. To change it, override `BuildConfiguration` in your `TestFixture`:

```cs
public class TestFixture : ServiceProviderTestFixture
{
    protected override IConfiguration BuildConfiguration()
        => new ConfigurationBuilder().AddJsonFile("mysettings.json").Build();

    protected sealed override void RegisterCoreDependencies(IServiceCollection services)
    {
        var settings = Configuration.Get<TestSettings>();
        // ...
    }
}
```

## Database setup

For database support use `Fusonic.Extensions.UnitTests.EntityFrameworkCore` (namespace `Fusonic.Extensions.UnitTests.EntityFrameworkCore`) together with
- `Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql` for PostgreSQL or
- `Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer` for Microsoft SQL Server.

Every test gets its own copy of a template database. This enables parallel database tests and keeps tests from affecting each other. The setup consists of three parts:
1. A [test startup](#test-startup) that starts the database server and creates the template once per test run.
2. An `ITestStore` registered as singleton, together with your `DbContext`, in the fixture.
3. A test base inheriting from `DatabaseUnitTest`.

### Database test base

`DatabaseUnitTest<TDbContext, TFixture>` uses `TDbContext` as default `DbContext`. Use `DatabaseUnitTest<TFixture>` if you do not want a default `DbContext`. Example base classes:

```cs
public abstract class TestBase(TestFixture fixture) : TestBase<TestFixture>(fixture);

public abstract class TestBase<TFixture>(TFixture fixture) : DatabaseUnitTest<AppDbContext, TFixture>(fixture)
    where TFixture : TestFixture;
```

`DatabaseUnitTest` provides the methods `Query` and `QueryAsync`. They resolve the `DbContext` in a separate scope and run your query. So instead of writing

```cs
var count = await ScopedAsync(() =>
{
    var context = GetInstance<AppDbContext>();
    return context.Documents.CountAsync();
});
```

you can just write

```cs
var count = await QueryAsync(ctx => ctx.Documents.CountAsync());
```

For other `DbContext` types, specify the type: `QueryAsync<OtherDbContext>(ctx => ...)` and `Query<OtherDbContext>(ctx => ...)` for queries without a result, `QueryAsync<OtherDbContext, int>(ctx => ...)` and `Query<OtherDbContext, int>(ctx => ...)` for queries with a result.

`DatabaseUnitTest` implements `IAsyncLifetime`. The test database is dropped in `DisposeAsync`. When you override `InitializeAsync` or `DisposeAsync`, call the base implementation.

### Test startup

Start the database server and create the template once per test run with an xUnit v3 [assembly fixture](https://xunit.net/docs/shared-context#assembly-fixture). [TestContainers](https://testcontainers.com/) is a simple way to get the database server running locally. In CI the database can be provided as service, passed in by an environment variable. Example for PostgreSQL:

```cs
[assembly: AssemblyFixture(typeof(TestStartup))]

public class TestStartup : IAsyncLifetime
{
    public static string ConnectionString { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // In CI the database is provided as a service. Locally, a test container is started.
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var container = new PostgreSqlBuilder("postgres:17-alpine")
                .WithReuse(true)
                .WithName("test.myproject")
                .WithTmpfsMount("/var/lib/postgresql/data") // Keep the database in memory, which speeds up the tests significantly.
                .WithTmpfsMount("/dev/shm")
                .Build();

            await container.StartAsync();
            connectionString = container.GetConnectionString();
        }

        // The database name is the name of the template.
        ConnectionString = new NpgsqlConnectionStringBuilder(connectionString) { Database = "myproject_test" }.ConnectionString;

        await PostgreSqlUtil.CreateTestDbTemplate<AppDbContext>(
            ConnectionString,
            o => new AppDbContext(o),
            seed: ctx => new TestDataSeed(ctx).Seed());
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
```

For SQL Server, use `MsSqlBuilder` from `Testcontainers.MsSql`, set `InitialCatalog` instead of `Database` and call `SqlServerTestUtil.CreateTestDbTemplate`.

`CreateTestDbTemplate` (both `PostgreSqlUtil` and `SqlServerTestUtil`) has these optional parameters:
- `npgsqlOptionsAction` / `sqlServerOptionsAction`: configures the provider, e.g. for `UseNetTopologySuite()`.
- `seed`: seeds the template after creation.
- `logger`: defaults to a console logger.
- `useMigrations`: applies the migrations (default). Set it to `false` to create the schema with `EnsureCreated`.
- `overwrite`: by default the template is only created if it does not exist yet, which speeds up local test runs. Set it to `true` to always create a fresh template. Without `overwrite`, delete the template (or the reused container) after changing the schema.

`PostgreSqlUtil` and `SqlServerTestUtil` also provide `CheckDatabaseExists` and `DropDatabase`.

### PostgreSQL

Register the `NpgsqlDatabasePerTestStore` with the template connection string. The database name in the connection string is the template name and must not be `postgres`. The test database is created lazily when the first connection is opened.

```cs
public class TestFixture : ServiceProviderTestFixture
{
    protected sealed override void RegisterCoreDependencies(IServiceCollection services)
    {
        var testStore = new NpgsqlDatabasePerTestStore(TestStartup.ConnectionString);
        services.AddSingleton<ITestStore>(testStore);

        services.AddDbContext<AppDbContext>(b => b.UseNpgsqlDatabasePerTest(testStore));
    }
}
```

`UseNpgsqlDatabasePerTest` has an optional `npgsqlOptionsAction` parameter to configure the provider.

When using `IDbContextFactory`, the factory must be registered with scoped lifetime, not with the default singleton lifetime:
```cs
services.AddDbContextFactory<AppDbContext>(b => b.UseNpgsqlDatabasePerTest(testStore), ServiceLifetime.Scoped);
```

### Microsoft SQL Server

Register the `SqlServerDatabasePerTestStore` with the template connection string. The `Initial Catalog` of the connection string is the template name and must not be `master`.

```cs
public class TestFixture : ServiceProviderTestFixture
{
    protected sealed override void RegisterCoreDependencies(IServiceCollection services)
    {
        var testStore = new SqlServerDatabasePerTestStore(TestStartup.ConnectionString);
        services.AddSingleton<ITestStore>(testStore);

        services.AddDbContext<AppDbContext>(b => b.UseSqlServerDatabasePerTest(testStore));
    }
}
```

SQL Server creates a backup of the template (`<template>.bak`) and restores it for each test. The files are stored in the data directory **on the SQL Server host**. It defaults to `/var/opt/mssql/data`, which matches the official Linux Docker image. Pass `dataDirectoryPath` to the constructor if your server uses another directory. The optional `databasePrefix` sets a prefix for the test database names.

`UseSqlServerDatabasePerTest` has an optional `sqlServerOptionsAction` parameter to configure the provider.

When using `IDbContextFactory`, the factory must be registered with scoped lifetime, not with the default singleton lifetime:
```cs
services.AddDbContextFactory<AppDbContext>(b => b.UseSqlServerDatabasePerTest(testStore), ServiceLifetime.Scoped);
```

### Database tests with SimpleInjector

`AddDbContext` is only available for `IServiceCollection`, so register the `DbContext` in a `ServiceCollection` and cross-wire it into SimpleInjector (requires `SimpleInjector.Integration.ServiceCollection`). `DatabaseUnitTest` resolves the `ITestStore` from the container, so it must be resolvable there as singleton.

```cs
public class TestFixture : SimpleInjectorTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        var services = new ServiceCollection();

        var testStore = new NpgsqlDatabasePerTestStore(TestStartup.ConnectionString);
        services.AddSingleton<ITestStore>(testStore);
        services.AddDbContext<AppDbContext>(b => b.UseNpgsqlDatabasePerTest(testStore));

        services.AddSimpleInjector(container, setup => setup.AutoCrossWireFrameworkComponents = true);
        services.BuildServiceProvider(validateScopes: true).UseSimpleInjector(container);
    }
}
```

### Configuring any other database

The database support is not limited to PostgreSQL and SQL Server. Implement and register an `ITestStore`:
- `OnTestConstruction()` is called in the constructor of each `DatabaseUnitTest`. Create a new database name for the test here.
- `OnTestEnd()` is called in `DisposeAsync` of the test. Drop the test database here.

To create the database lazily when the first connection opens, add `new ConnectionOpeningInterceptor(testStore.CreateDatabase)` to the `DbContextOptionsBuilder` with `AddInterceptors`. For an example with SQLite, see [SqliteTestStore](https://github.com/fusonic/dotnet-extensions/blob/main/src/UnitTests.EntityFrameworkCore/test/SqliteTestStore.cs) and its [TestFixture](https://github.com/fusonic/dotnet-extensions/blob/main/src/UnitTests.EntityFrameworkCore/test/TestFixture.cs).

### Support multiple databases in a test

You can test with multiple database systems at once. Instead of registering the test stores one by one, combine them in an `AggregateTestStore`:

```cs
public class TestFixture : ServiceProviderTestFixture
{
    protected sealed override void RegisterCoreDependencies(IServiceCollection services)
    {
        var npgsqlTestStore = new NpgsqlDatabasePerTestStore(TestStartup.NpgsqlConnectionString);
        services.AddDbContext<NpgsqlDbContext>(b => b.UseNpgsqlDatabasePerTest(npgsqlTestStore));

        var sqlServerTestStore = new SqlServerDatabasePerTestStore(TestStartup.SqlServerConnectionString);
        services.AddDbContext<SqlServerDbContext>(b => b.UseSqlServerDatabasePerTest(sqlServerTestStore));

        services.AddSingleton<ITestStore>(new AggregateTestStore(npgsqlTestStore, sqlServerTestStore));
    }
}
```

## Running in GitLab

There are two options to provide the database in GitLab CI.

**Database as service:** Start the database as a service and pass the connection string as environment variable. The [test startup](#test-startup) above uses it instead of starting a container:

```yaml
dotnet:test:
  services:
    - name: postgres:17-alpine
      alias: postgres
  variables:
    POSTGRES_PASSWORD: postgres
    ConnectionStrings__Postgres: "Host=postgres;Username=postgres;Password=postgres"
```

**TestContainers in Docker-in-Docker:** Start `docker:dind-rootless` as a service. When running rootless dind, you also must set `TESTCONTAINERS_RYUK_DISABLED` to `true`, as there is no `docker.sock` available. Ryuk is responsible for cleaning up the test containers, even when test jobs get cancelled. Disabling it is safe, as the containers started within `docker:dind` get cleaned up after the job ends anyway.

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
