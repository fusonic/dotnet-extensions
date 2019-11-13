# Unit tests

- [Unit tests](#unit-tests)
  - [Introduction](#introduction)
  - [Setup](#setup)
  - [Test scoped lifestyle](#test-scoped-lifestyle)
  - [Test settings / Microsoft.Extensions.Configuration](#test-settings--microsoftextensionsconfiguration)
  - [Database provider setup](#database-provider-setup)
    - [Fixture](#fixture)
    - [Test base](#test-base)
  - [Supported providers](#supported-providers)
    - [NoDb](#nodb)
    - [InMemory](#inmemory)
    - [PostgreSql](#postgresql)
      - [CLI - Test DB Templates](#cli---test-db-templates)
      - [Cleanup](#cleanup)
    - [SQL Server](#sql-server)

## Introduction

The unit test framwork tries to provide the following features & design goals:

- **Simplicity:** The unit test base classes provide several helper methods targeted to support you using our currently used default architectural structures and libraries. To reduce reading and writing overhead (clutter) the methods tend have short, less descriptive names. For example, `Scoped` instead of `RunInSeparateLifetimeScope`, but they aren't that much and are easy to learn.
  - **Resolving types:** Can be done via the Container property or dirctly using `GetInstance<T>()`
  - **Scope separation:** In reality, creating data and consuming that data is not done in the same scope. In order to be able to see issues when using different scopes in the unit tests you can run your code in the provide `Scoped` and `ScopedAsync` methods. They run your code in separate lifetime scopes of the SimpleInjector-Container. Within the tests you usually want to run your data preparations and your queries in different scopes.
  - **MediatR support:** Send your MediatR-requests in a separate scope by simply calling `SendAsync<TResponse>`.
  
- **Database independency:** Previously we used an `XyzTest`, `XyzDbTest`, `XyzInMemoryTest`. In a nutshell the only difference: the accessed Database. It was either none, InMemory or a real database in the background. An additional issue: Test data. Proper test data was only in the DB tests, as setting it up can be quite cumbersome. So in the end the non-db tests were hardly ever used for testing logic, everything was in the slower DbTests. With this framework you can define the database provider **per test**, without using separate classes, using a simple attribute. You can also specify a default per class by setting the attribute on the class and globally by configuring it in your test base.

- **Dependency injection:** The test base is layed out to be used with SimpleInjector as dependency injection framework. Services can be directly resolved from the test class. (Constructor injection is not supported, but could be easily extended if required.)
  - **Dependencies per test:** Most services are fine to be resolved within the scopes provided by SimpleInjector. However, some unit test framework services depend on the test method (`DatabaseProvider`) or require one instance to span multiple scopes in one test. You can register such dependencies by using the `TestScopedLifestyle` from the fixture.

- **Extensibility:** The basic framework, `Fusonic.Extensions.UnitTests` does not come with any support for Databases and we don't want it. There are lots of libs without any database access and we don't want to pollute those tests with EF core dependencies. Also multiple providers, like InMemory, Postgres and (currently unsupported) SQL Server are available. Those dependencies should be easily available, but should not be required.

- **Configuration support:** Microsoft.Extensions.Configuration is supported out of a box with usable default settings.

- **TestContext:** More or less a hidden feature, but useful for framework code. A test context is provided with info about the executing method and class and access to the test output helper.

## Setup

The intention is that you have one base class and one base fixture per test assembly. You don't have multiple classes for use cases, where the only difference is a resolved service (like in memory or postgres database).

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
        //Call the base for the core dependencies.
        base.RegisterCoreDependencies(container);

        //Your core dependencies here.
    }
}
```

The test class is also abstract and requires a fixture as type parameter. Create the following base classes:

```cs
public abstract class TestBase<TFixture> : UnitTest<TFixture>
    where TFixture : TestFixture
{
    protected TestBase(TestFixture fixture) : base(fixture)
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

## Database provider setup

### Fixture

As mentioned above, for database support you don't create your own class. Just inherit from a different fixture and test base. An example:

```cs
public class TestFixture : DatabaseFixture<YourDbContext>
{
    protected override void ConfigureDatabaseProviderss(DatabaseFixtureConfiguration<YourDbContext> configuration)
    {
        configuration.UseInMemoryDatabase(seed: ctx => new TestDataSeed(ctx).Seed())
                     .UsePostgreSqlDatabase(connectionString: "...", dbNamePrefix: "YourProjectTest", templateDb: "YourProjectTest_Template")
                     .UseDefaultProviderAttribute(new InMemoryTestAttribute());
    }

    protected override void RegisterDependencies(Container container)
    {
        //Your dependencies here
    }
}
```

- Inherit from `DatabaseFixture<YourDbContext>`
- Configure the database providers
- Register dependencies if required

The database providerss are responsible for creating a `DbContext`, creating the database itself and dropping the database. The providers must be able to handle multiple test-threads at once, meaning that each provider should cater a different database.

The providers are instantiated once per test or not at all, if no database connection is requested.

In the example above `ConfigureDatabaseProviders` does configure the following:

- We support InMemory-Database tests. They are marked with `[InMemoryTest]`. The `TestDataSeed` gets executed for each one of these tests.
- We support database tests against an PostgreSql-Database. They are marked with `[PostgreSqlTest]`. The database gets created from an already seeded database template, thus the seed must not be run again. You can find the parameter descriptions in [PostgreSql](#postgresql).
- If no attribute is defined on the method or the class, use an InMemory-database

  When a db context is requested, `YourDbContext` gets resolved, so you can inject other dependencies into your context. The `DbContextOptions` provided depend on the current test (InMemoryOptions vs PostgreSqlOptions vs WhateverOptions).

### Test base

The only difference is that your `TestBase` has to inherit from `DatabaseUnitTest<TDbContext, TFixture>`:

```cs
public abstract class TestBase : TestBase<TestFixture>
{
    protected TestBase(TestFixture fixture) : base(fixture)
    { }
}
public abstract class TestBase<TFixture> : DatabaseUnitTest<YourDbContext, TFixture>
    where TFixture : TestFixture
{
    protected TestBase(TestFixture fixture) : base(fixture)
    { }
}
```

You'll get some new supporting methods to call:

- `Query` and `QueryAsync` are basically the same as `Scoped`, but you get a DbContext as parameter.
- `GetDbContext` to get the db context

## Supported providers

### NoDb

This one does not configure the DbContext at all. You can use it to ensure that no DB gets created or just to skip the DB overhead when you don't need it. You don't need any extra configuration for it. Mark a test or a test class with `[NoDatabase]` to use it.

### InMemory

For the EF Core InMemory-Database, configure the fixture with `configuration.UseInMemoryDatabase()`. Mark a test or a test class with `[InMemoryTest]` to use it.

**Parameters:**

|Parameter|Description|
|-|-|
|**seed**|The seed that should be executed when this provider is used.|

### PostgreSql

For PostgreSql support, configure the fixture with `configuration.UsePostgreSqlDatabase(...)`. Mark a test or a test class with `[PostgreSqlServer]` to use it.

**Parameters:**

|Parameter|Description|
|-|-|
|**connectionString**|The connection string points to the `postgres` database. You don't know the name of the test DB in this context. The provider takes care of that.|
|**dbNamePrefix**|All test databases are created with the prefix (ie. `YourProjectTest`). Use the branch name or sth. like that when running in a CI pipeline.|
|**templateDb**|When a template is defined, the test database will initially be copied from it (ie. `YourProjectTest_Template`). When a template is set, migrations won't be executed. The template is expected to be up to date. Use `PostgreSqlUtil` (see below) for template creation support.|
|**optionsBuilder**|The options builder for Npgsql if the context requires one. Gets used in `DbContextOptionsBuilder<TDbContext>().UseNpgsql()`. Example: `o => o.UseNodaTime()`.|
|**seed**|The seed that should be executed when this provider is used.|

The lib also features a fancy `PostgreSqlUtil`. Use it in your CI scripts and for local testing. For example, a LinqPad-script for creating a local DB template for unit tests could look like this:

```cs
string connectionString = "Host=localhost;Database=YourProjectTest_Template;Username=postgres;Password=postgres";

"Cleanup:".Dump();
PostgreSqlUtil.Cleanup(connectionString, "YourProjectTest");

"\r\nTemplate:".Dump();
PostgreSqlUtil.CreateTestDbTemplate<YourDbContext>(connectionString, o => new YourDbContext(o), o => o.UseNodaTime(), c => new TestDataSeed(c).Seed());
```

Alternatively, if you have implemented an `ITestDbTemplateCreator` as [outlined below](#cli---test-db-templates), you can just use it via LinqPad or using the `pgtestutil`.

**Attribute parameters:**

The `PostgreSqlTestAttribute` has the following parameters:

- **EnableLogging:** EF Core output will be logged to the Xunit test output if set to `true`. Default: `false`

#### CLI - Test DB Templates

If you have a database to test against, it is probably a good idea to use test templates. They are already migrated to the current state and pre-seeded, so every database test is good to go from the start.

To manage the test databases via CLI there is a tool called `pgtestutil`. Install it using the command

```bash
dotnet tool install --global Fusonic.Extensions.UnitTests.Tools.PostgreSql
```

You then can access it via the command `pgtestutil`. Use `pgtestutil -h` or `pgtestutil help <command>` to get help about the detailed usage of this tool.

For creating a test database template with this tool you have to implement `ITestDbTemplateCreator`. The tool instantiates the implementation to create the template. The implementation should be straight forward as you can utilize `PostgreSqlUtil`. It could look like this:

```cs
public class TestDbTemplateCreator : ITestDbTemplateCreator
{
    public void Create(string connectionString)
    {
        //The connection string contains the test db name that gets used as prefix.
        var dbName = PostgreSqlUtil.GetDatabase(connectionString);

        //Drop all databases that may still be there from previously stopped tests.
        PostgreSqlUtil.Cleanup(connectionString, dbPrefix: dbName);

        //Create the template
        PostgreSqlUtil.CreateTestDbTemplate<HomeDbContext>(connectionString, o => new HomeDbContext(o), seed: c => new TestDataSeed(c).Seed());
    }
}
```

This then can be called with `pgtestutil template -c "ConnectionString" -a Path\To\Your\Assembly.dll`

#### Cleanup

Normally the tests and CI jobs do a fine job of cleaning everything up. However, if for example you just stop an environment while a test is running and delete it then, it may happen that there are remains of unused test databases on the RDS server. To get rid of them, there is a template for a nightly task available. See `Cleanup-TestDatabases` in our project `gitlab-ci-tools` for details.

### SQL Server

Not supported yet :(
