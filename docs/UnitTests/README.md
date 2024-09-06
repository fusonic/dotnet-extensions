# Unit tests

- [Unit tests](#unit-tests)
  - [Introduction](#introduction)
  - [Setup](#setup)
  - [Test settings / Microsoft.Extensions.Configuration](#test-settings--microsoftextensionsconfiguration)
  - [Database setup](#database-setup)
    - [Test base](#test-base)
    - [PostgreSQL - Configure DbContext](#postgresql---configure-dbcontext)
    - [PostgreSQL - Template](#postgresql---template)
      - [PostgreSQL template option: Create it in the fixture](#postgresql-template-option-create-it-in-the-fixture)
      - [PostgreSQL template option: Console application](#postgresql-template-option-console-application)
    - [Microsoft SQL Server - Configure DbContext](#microsoft-sql-server---configure-dbcontext)
    - [Configuring any other database](#configuring-any-other-database)
    - [Support mulitple databases in a test](#support-mulitple-databases-in-a-test)
    - [Database test concurrency](#database-test-concurrency)

## Introduction

The unit test framwork tries to provide the following features & design goals:

- **Simplicity:** The unit test base classes provide several helper methods targeted to support you using our currently used default architectural structures and libraries. To reduce reading and writing overhead (clutter) the methods tend have short, less descriptive names. For example, `Scoped` instead of `RunInSeparateLifetimeScope`, but they aren't that much and are easy to learn.
  - **Resolving types:** Can be done using `GetInstance<T>()`
  - **Scope separation:** In reality, creating data and consuming that data is not done in the same scope. In order to be able to see issues when using different scopes in the unit tests you can run your code in the provided `Scoped` and `ScopedAsync` methods. They run your code in separate lifetime scopes of the DI-Container. Within the tests you usually want to run your data preparations and your queries in different scopes.

- **Dependency injection:** The test base is layed out to be support dependency injection. Use `Fusonic.Extensions.UnitTests.ServiceProvider` to use Microsofts dependency injection, or `Fusonic.Extensions.UnitTests.SimpleInjector` for SimpleInjector support.

- **Database support:** The basic framework, `Fusonic.Extensions.UnitTests` does not come with any support for Databases, but we do provide support for fast and parallel database tests with `Fusonic.Extensions.UnitTests.EntityFrameworkCore`, currently specifically supporting PostgreSQL with `Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql`.

- **Configuration support:** Microsoft.Extensions.Configuration is supported out of a box with usable default settings.

## Setup

Currently SimpleInjector and Microsofts dependency injection are supported. Reference the required library accordingly:
- `Fusonic.Extensions.UnitTests.SimpleInjector` or
- `Fusonic.Extensions.UnitTests.ServiceProvider`

Create a `TestBase` and a `TestFixture` for the assembly.

The `TestFixture` is used for registering your depdendencies. Override `ServiceProviderTestFixture` or `SimpleInjectorTestFixture`, depending on the DI container you want to use.

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

For database support you can use `Fusonic.Extensions.UnitTests.EntityFrameworkCore`, optionally with specific support for PostgreSQL in `Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql`.

The basic idea behind those is that every test gets its own database copy. This enables parallel database testing and avoids any issues from tests affecting other tests.

### Test base

A test base for your database tests is available called `DatabaseUnitTest`. Type parameters are the test fixture optionally the `DbContext`. Example base classes in your test lib:
```cs
public abstract class TestBase : TestBase<TestFixture>
{
    protected TestBase(TestFixture fixture) : base(fixture)
    { }
}

public abstract class TestBase<TFixture> : DatabaseUnitTest<AppDbContext, TFixture>
    where TFixture : TestFixture
{
    protected TestBase(TFixture fixture) : base(fixture)
    { }
}
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
        var options = new NpgsqlDatabasePerTestStoreOptions
        {
            ConnectionString = Configuration.GetConnectionString("Npgsql")
        };
        var testStore = new NpgsqlDatabasePerTestStore(options);
        services.AddSingleton<ITestStore>(testStore);

        services.AddDbContext<AppDbContext>(b => b.UseNpgsqlDatabasePerTest(testStore));
    }
}
```

The interface of `ITestStore` is straight forward. You can easily replace your test store with something else for another strategy or for supporting other databases.

### PostgreSQL - Template

When using the `NpgsqlDatabasePerTest` it is assumed that you use a prepared database template. This template should have all migrations applied and may contain some seeded data. Each test gets a copy of this template. With the `PostgreSqlUtil`, we provide an easy way to create such a template.

You can either create a small console application that creates the template, or do it directly once in the fixture during setup.

#### PostgreSQL template option: Create it in the fixture

You can create the template directly in the TestFixture by specifying a `TemplateCreator` in the options:

```cs
protected sealed override void RegisterCoreDependencies(ServiceCollection services)
{
    var options = new NpgsqlDatabasePerTestStoreOptions
    {
        ConnectionString = Configuration.GetConnectionString("Npgsql"),
        TemplateCreator = CreateTemplate; // Function to create the template on first connect
    };
    
    // rest of the configuration
}

private static Task CreateTemplate(string connectionString)
    => PostgreSqlUtil.CreateTestDbTemplate<AppDbContext>(connectionString, o => new AppDbContext(o), seed: ctx => new TestDataSeed(ctx).Seed());
```

By default, if the template creator is set, the `TestStore` checks exactly once, if the database exists.
- If the template database exists, no action will be taken. It is not checked, if the database is up to date.
- If the template database does not exist, the `TemplateCreator` is executed.
- All future calls won't do anything and just return.

`PostgreSqlUtil.CreateTestDbTemplate` force drops and recreates your database. However, it won't be called if the datbase already exists.

In order to get updates to your test database, either drop it or restart your postgresql container, if its data partition is mounted to `tmpfs`.

You can change this behavior to always create a template by setting `options.AlwaysCreateTemplate` to true. In that case, the `TemplateCreator` will always be executed once per test run. This will increase the startup time for your test run though.

#### PostgreSQL template option: Console application  
Alternatively, if you prefer to create the test database externally before the test run, create a console application with the following code in `Program.cs`:

```cs
if (args.Length == 0)
{
    Console.Out.WriteLine("Missing connection string.");
    return 1;
}

PostgreSqlUtil.CreateTestDbTemplate<AppDbContext>(args[0], o => new AppDbContext(o), seed: ctx => new TestDataSeed(ctx).Seed());

return 0;
```

With that, the database given in the connection string is getting force dropped, recreated, migrations applied and optionally seeded via the given `TestDataSeed`. You can simply call it in your console or the build pipeline before running the tests using 
```sh
dotnet run --project <pathToCsProject> "<connectionString>"
```

### Microsoft SQL Server - Configure DbContext

A `TestStore` is used for handling the test databases. For Microsoft SQL Server, you can use the `SqlServerDatabasePerTestStore`, which creates a separate database for each test. You have to pass the connection string to the database and a method to create the test database. Register it as follows:

```cs
public class TestFixture : ServiceProviderTestFixture
{
    protected sealed override void RegisterCoreDependencies(ServiceCollection services)
    {
        var options = new SqlServerDatabasePerTestStoreOptions
        {
            ConnectionString = Configuration.GetConnectionString("SqlServer")!,
            TemplateCreator = CreateSqlServerTemplate,
            DatabasePrefix = "project_test_" // Optional. Defines a prefix for the randomly generated test database names.
            DatabaseDirectoryPath = "C:/mssql/data" // Optional. Defaults to docker image default path.
        };
        var testStore = new SqlServerDatabasePerTestStore(options);
        services.AddSingleton<ITestStore>(testStore);
        services.AddDbContext<AppDbContext>(b => b.UseSqlServerDatabasePerTest(testStore));
    }

    private static async Task CreateSqlServerTemplate(string connectionString)
        => await SqlServerTestUtil.CreateTestDbTemplate<SqlServerDbContext>(connectionString, o => new SqlServerDbContext(o));
}
```

The connection string must have the `Intial catalog` set. It determines the name of the template database. All tests will use a copy of the template database.

The `TemplateCreator` specifies the method to create a template. It has to create and seed the database and create a backup for the copies used for the tests. Fortunately, the `SqlServerTestUtil` provides a method to do exactly that.


### Configuring any other database

The database support is not limited to PostgreSql and SQL Server. You just have to implement and register the `ITestStore`.

For a simple example with SqLite, check `Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests` -> `SqliteTestStore` and `TestFixture`.

### Support mulitple databases in a test

You can test with multiple, different database systems at once. The setup stays basically the same, but instead of registering the test stores one by one, use the `AggregateTestStore`. Example:

```cs
public class TestFixture : ServiceProviderTestFixture
{
    private void RegisterDatabase(IServiceCollection services)
    {
        // Register Npgsql (PostgreSQL)
        var npgsqlSettings = new NpgsqlDatabasePerTestStoreOptions
        {
            ConnectionString = Configuration.GetConnectionString("Npgsql"),
            TemplateCreator = CreatePostgresTemplate
        };

        var npgsqlTestStore = new NpgsqlDatabasePerTestStore(npgsqlSettings);
        services.AddDbContext<NpgsqlDbContext>(b => b.UseNpgsqlDatabasePerTest(npgsqlTestStore));

        // Register SQL Server
        var sqlServerSettings = new SqlServerDatabasePerTestStoreOptions
        {
            ConnectionString = Configuration.GetConnectionString("SqlServer")!,
            TemplateCreator = CreateSqlServerTemplate
        };
        var sqlServerTestStore = new SqlServerDatabasePerTestStore(sqlServerSettings);
        services.AddDbContext<SqlServerDbContext>(b => b.UseSqlServerDatabasePerTest(sqlServerTestStore));

        // Combine the test stores in the AggregateTestStore
        services.AddSingleton<ITestStore>(new AggregateTestStore(npgsqlTestStore, sqlServerTestStore));
    }

    private static Task CreatePostgresTemplate(string connectionString)
        => PostgreSqlUtil.CreateTestDbTemplate<NpgsqlDbContext>(connectionString, o => new NpgsqlDbContext(o), seed: ctx => new TestDataSeed(ctx).Seed());

    private static Task CreateSqlServerTemplate(string connectionString)
        => SqlServerTestUtil.CreateTestDbTemplate<SqlServerDbContext>(connectionString, o => new SqlServerDbContext(o));
}
```

### Database test concurrency

XUnit limits the number the maximum _active_ tests executing, but it does not the limit of maximum parallel tests.  
Simplified, as soon as a test awaits a task somewhere, the thread is returned to the pool and another test gets started. This is intended by design.  

This behavior can cause issues when running integration tests against a database, especially when lots of tests are started. Connection limits can be exhausted quickly.

To solve this, you can either throttle your tests, or increase the max. connections of your test database.

To increase the max. connections of your postgres test instance, just pass the parameter max_connections. Example for a docker compose file:
```yaml
postgres_test:
  image: postgres:14
  command: -c max_connections=300
  ports:
    - "5433:5432"
  volumes:
    - type: tmpfs
      target: /var/lib/postgresql/data
    - type: tmpfs
      target: /dev/shm
  environment:
    POSTGRES_PASSWORD: developer
```

Alternatively, if you want to throttle your tests instead, you can to this easily with a semaphore in your test base:

```cs
public class TestBase : IAsyncLifetime

    private static readonly SemaphoreSlim Throttle = new(64);
    public async Task InitializeAsync() => await Throttle.WaitAsync();

    public virtual Task DisposeAsync()
    {
        _ = Throttle.Release();
        return Task.CompletedTask;
    }
}
```
