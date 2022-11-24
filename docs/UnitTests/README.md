# Unit tests

- [Unit tests](#unit-tests)
  - [Introduction](#introduction)
  - [Setup](#setup)
  - [Test scoped lifestyle](#test-scoped-lifestyle)
  - [Test settings / Microsoft.Extensions.Configuration](#test-settings--microsoftextensionsconfiguration)
  - [Database setup](#database-setup)
    - [Test base](#test-base)
    - [PostgreSQL - Template](#postgresql---template)
    - [PostgreSQL - Configure single DbContext](#postgresql---configure-single-dbcontext)
    - [PostgreSQL - Configure multiple DbContexts with same database](#postgresql---configure-multiple-dbcontexts-with-same-database)
    - [PostgreSQL - Configure multiple DbContexts with different databases](#postgresql---configure-multiple-dbcontexts-with-different-databases)
    - [Configuring any other database](#configuring-any-other-database)
    - [Database test concurrency](#database-test-concurrency)
  - [Samples](#samples)

## Introduction

The unit test framwork tries to provide the following features & design goals:

- **Simplicity:** The unit test base classes provide several helper methods targeted to support you using our currently used default architectural structures and libraries. To reduce reading and writing overhead (clutter) the methods tend have short, less descriptive names. For example, `Scoped` instead of `RunInSeparateLifetimeScope`, but they aren't that much and are easy to learn.
  - **Resolving types:** Can be done via the Container property or dirctly using `GetInstance<T>()`
  - **Scope separation:** In reality, creating data and consuming that data is not done in the same scope. In order to be able to see issues when using different scopes in the unit tests you can run your code in the provided `Scoped` and `ScopedAsync` methods. They run your code in separate lifetime scopes of the SimpleInjector-Container. Within the tests you usually want to run your data preparations and your queries in different scopes.
  - **MediatR support:** Send your MediatR-requests in a separate scope by simply calling `SendAsync<TResponse>`.

- **Dependency injection:** The test base is layed out to be used with SimpleInjector as dependency injection framework. Services can be directly resolved from the test class. (Constructor injection is not supported, but could be easily extended if required.)
  - **Dependencies per test:** Most services are fine to be resolved within the scopes provided by SimpleInjector. However, some unit test framework services depend on the test method (`DatabaseProvider`) or require one instance to span multiple scopes in one test. You can register such dependencies by using the `TestScopedLifestyle` from the fixture.

- **Database support:** The basic framework, `Fusonic.Extensions.UnitTests` does not come with any support for Databases, but we do provide support for fast and parallel database tests with `Fusonic.Extensions.UnitTests.EntityFrameworkCore`, currently specifically supporting PostgreSQL with `Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql`.

- **Configuration support:** Microsoft.Extensions.Configuration is supported out of a box with usable default settings.

- **TestContext:** Useful for framework code, the test context provides info about the executing method and class and access to the test output helper. You can also put own objects into the context that span the lifetime of the test.

## Setup

The intention is that you have one base class and one base fixture per test assembly. You don't have multiple classes for use cases where the only difference is a resolved service.

To activate the features of the lib (like the `TestContext` and the per-test-database) you need to create an `AssemblyInfo.cs` with the following contents:

```cs
[assembly: Fusonic.Extensions.UnitTests.XunitExtensibility.FusonicTestFramework]
```

The setup of the `TestBase` and the `TestFixture` is quick and easy. In `RegisterCoreDependencies` you register those dependencies that you usually need for your tests (MediatR, Services, ASP.Net services and the like). It should not be overwritten in your specific tests, so consider to make it `sealed`. For your test specific fixtures use `RegisterDependencies` instead.

```cs
public class TestFixture : UnitTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        //Your core dependencies here.
    }
}
```

The test class is also abstract and requires a fixture as type parameter. Create the following base classes:

```cs
public abstract class TestBase<TFixture> : UnitTest<TFixture>
    where TFixture : TestFixture
{
    protected TestBase(TFixture fixture) : base(fixture)
    { }
}

public abstract class TestBase : TestBase<TestFixture>
{
    protected TestBase(TestFixture fixture) : base(fixture)
    { }
}
```

That's it, you're good to go. Inherit your classes from the `TestBase` and get all the fancy features. If you need own fixtures, inherit them from `TestFixture` and your test class from `TestBase<TFixture>`. Register your dependencies by overriding `RegisterDependencies`.

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

## Test scoped lifestyle

There is a lifetime scope available that is valid within one test. For example, the `DatabaseProvider` needs to be one instance per test and a new instance on the next.

If you have such a requirement, register your serivce with the `TestScopedLifestyle`:

```cs
protected override void RegisterDependencies(Container container)
{
    //Note: the usual overrides for the register methods are available too,
    //like RegisterTestScoped<TService, TImplementation>() and RegisterTestScoped<TService>(Func<TService>)
    container.RegisterTestScoped<YourService>();
}
```

## Test settings / Microsoft.Extensions.Configuration

That's the default configuration:

```cs
public static IConfiguration GetDefaultConfiguration(string basePath, Assembly assembly)
{
    return new ConfigurationBuilder()
          .SetBasePath(basePath)
          .AddJsonFile("testsettings.json", optional: true)
          .AddUserSecrets(assembly, optional: true)
          .AddEnvironmentVariables()
          .Build();
}

protected virtual IConfiguration BuildConfiguration() => GetDefaultConfiguration(Directory.GetCurrentDirectory(), GetType().Assembly);
```

If you want to, overwrite it in your `TestBase` by overwriting `BuildConfiguration`. You can also bind your test settings objects like this:

```cs
public TestSettings TestSettings { get; } = new TestSettings();

protected override IConfiguration BuildConfiguration()
{
    var configuration = base.BuildConfiguration();
    configuration.Bind(TestSettings);
    return configuration;
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

    protected override void DropTestDatabase() => GetInstance<NpgsqlTestDatabaseProvider>().DropDatabase();
}
```

Note the `DropTestDatabase`: This method gets called when a test completes (successfully or unsuccessfully). The database for the test gets dropped then, as it is not required anymore. If you do not wish to use the `DatabaseUnitTest`-base, you have to take care about cleanup yourselfes.

Furhtermore, the `DatabaseUnitTest` provides the methods `Query` and `QueryAsync`. This is basically shortcut to resolving the `DbContext` and using it. So instead of writing

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

### PostgreSQL - Template

For PostgreSQL it is assumed that you use a prepared database template. This template should have all migrations applied and may contain some seeded data. Each test gets a copy of this template. With the `PostgreSqlUtil`, we provide an easy way to create such a template. Create a console application with the following code in `Program.cs`:

```cs
if (args.Length == 0)
{
    Console.Out.WriteLine("Missing connection string.");
    return 1;
}

PostgreSqlUtil.CreateTestDbTemplate<TestDbContext>(args[0], o => new AppDbContext(o), seed: ctx => new TestDataSeed(ctx).Seed());

return 0;
```

With that, the database given in the connection string is getting force dropped, recreated, migrations applied and optionally seeded via the given `TestDataSeed`. You can simply call it in your console or the build pipeline before running the tests using 
```sh
dotnet run --project <pathToCsProject> "<connectionString>"
```

### PostgreSQL - Configure single DbContext

In case you only have one `DbContext`, you can simply configure the database tests like follows:

```cs
public class TestFixture : UnitTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        container.RegisterNpgsqlDbContext<AppDbContext>(Configuration.Bind);
    }
}
```

`RegisterNpgsqlDbContext` registers your `DbContext` and all services for triggering the creation and deletion of the test databases when required.

**Settings:** 
The settings bound via `Configuration.Bind` are the `NpgsqlTestDatabaseProviderSettings`, which require the connection string to the test database template in the setting `TemplateConnectionString`. Provide this setting via an environment variable and/or using a `testsettings.json`.

That's it. Each of your tests now gets a fresh database, if required. It does not unnecessarily create databases, if there is no database access with in a test.

### PostgreSQL - Configure multiple DbContexts with same database

If you have for example a DbContext for writing and another one for reading (usually with a different connection string in the deployed application), you might
want to register them using the same connection string in the tests, as you usually don't start multpile replicated PostgreSQL-servers for testing.

```cs
public class TestFixture : UnitTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        container.RegisterNpgsqlDbContext<AppDbContext>(Configuration.Bind);
        container.RegisterNpgsqlDbContext<ReadOnlyDbContext>(Configuration.Bind);
    }
}
```

Both DbContexts get the same connection string. In your test base you might want to override `DatabaseUnitTests` using your write-DbContext as default for easier test data setup.

Note: If you configure the `ReadOnlyDbContext` with a different connection string, in this scenario you'd overwrite the configuration of `AppDbContext`.

### PostgreSQL - Configure multiple DbContexts with different databases

If you have multpile DbContexts requiring different database connections, you need to create overwrites for the database provider and its settings. The `NpgsqlTestDatabaseProvider` is responsible for creating and deleting the test databases that are provided for each test and used as default when you call `RegisterNpgsqlDbContext` with one type parameter.

```cs
public class TestFixture : UnitTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        container.RegisterNpgsqlDbContext<AppDbContext>(Configuration.Bind);
        container.RegisterNpgsqlDbContext<OtherDbContext, 
                                          OtherNpgsqlTestDatabaseProvider, 
                                          OtherNpgsqlTestDatabaseProviderSettings>(Configuration.GetSection("OtherDatabase").Bind);
    }
}

public class OtherNpgsqlTestDatabaseProvider : NpgsqlTestDatabaseProvider 
{
    public OtherNpgsqlTestDatabaseProvider(OtherNpgsqlTestDatabaseProviderSettings settings) : base(settings)
    { }
}
public class OtherNpgsqlTestDatabaseProviderSettings : NpgsqlTestDatabaseProviderSettings { }
```

Now both contexts get different connection strings (via the `NpgsqlTestDatabaseProviderSettings`) and their own test database.

### Configuring any other database

The database support is not limited to PostgreSql, `RegisterNpgsqlDbContext` is just further calling `RegisterDbContext` from `Fusonic.Extensions.UnitTests.EntityFrameworkCore`.

For supporting other databases, you just need to implement the `ITestDatabaseProvider`. It is responsible for
- Supplying a test database name (usually unique per test)
- Create the test database with that name
- Drop the test database with that name

With that done, you can just use `container.RegisterDbContext<AppDbContext, YourTestDatbaseProvider>(...)` to fully support your database.

For a simple example with SqLite, check `Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests` -> `TestDatabaseProvider` and `TestFixture`.

### Database test concurrency

XUnit limits the number the maximum _active_ tests executing, but it does not the limit of maximum parallel tests.  
Simplified, as soon as a test awaits a task somewhere, the thread is returned to the pool and another test gets started. This is intended by design.  

This behavior can cause issues when running integration tests against a database, especially when lots of tests are started. Connection limits can be exhausted quickly and other issues, like timeouts due to overload, may occur.

Our framework provides a possibility to limit the max. number of tests executing in parallel. This can be done in two ways:
* Set `MaxParallelTests` on the `FusonicTestFramework` assembly attribute, or
* set the environment variable `MAX_PARALLEL_TESTS`

If both are set, the environment variable has the higher precedence.

Note that this setting is not affecting the connection limit of entity framework or any other connection limits. Entity framework or this unit testing framework could still have more open connections than the MaxParallelTests setting, but it still can be leveraged to drastically reduce the chance of connection limit exhaustion and timeouts due to a too high load.

The following values are supported:
* `maxParallelTests < 0` disables the limits completly. Tests get executed as XUnit intends to.
* `maxParallelTests = 0` sets the limit to the virtual processor count of the machine. This is the default.
* `maxParallelTests > 0` sets the limit to the given number.

## Samples

There's a series of blog posts released about how to do fast unit testing, also related to this library. It is split into three parts:
 - [Fast unit tests with databases, part 1 – a primer](https://www.fusonic.net/de/blog/fast-unit-tests-with-databases-part-1)
 - [Fast unit tests with databases, part 2 – Introduction to the fusonic testing framework](https://www.fusonic.net/de/blog/fast-unit-tests-with-databases-part-2)
 - [Fast unit tests with databases, part 3 – Implementation of our solution](https://www.fusonic.net/de/blog/fast-unit-tests-with-databases-part-3)

Related to that there's a [sample project in this repository](../../samples/UnitTests/).
