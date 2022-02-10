// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

public class DatabaseFixtureConfiguration<TDbContext>
    where TDbContext : DbContext
{
    internal Dictionary<Type, Func<DatabaseProviderAttribute, ITestDatabaseProvider<TDbContext>>> ProviderFactories { get; } = new();
    internal DatabaseProviderAttribute? DefaultProviderAttribute { get; private set; }
    internal Func<DatabaseProviderAttribute, DatabaseProviderAttribute>? ReplaceProvider { get; private set; }

    public DatabaseFixtureConfiguration()
    {
        ProviderFactories[typeof(NoDatabaseAttribute)] = _ => new NoDatabaseProvider<TDbContext>();
    }

    /// <summary> Registers a provider for a specific attribute. Normally you don't call this in your unit test base as there should be proper extensions from the providers themselves. (like .UseInMemoryDatabase()) </summary>
    public DatabaseFixtureConfiguration<TDbContext> RegisterProvider(Type attributeType, Func<DatabaseProviderAttribute, ITestDatabaseProvider<TDbContext>> providerFactory)
    {
        if (!typeof(DatabaseProviderAttribute).IsAssignableFrom(attributeType))
            throw new InvalidOperationException($"Attribute must inherit from {nameof(DatabaseProviderAttribute)}");

        if (ProviderFactories.ContainsKey(attributeType))
            throw new InvalidOperationException("Provider is already defined for that attribute.");

        ProviderFactories[attributeType] = providerFactory;
        return this;
    }

    /// <summary> Define a default attribute for DB access when no attribute is specified on the class or method. Note that the attribute type must be registered using RegisterProvider, or else resolving a DbContext will fail. </summary>
    public DatabaseFixtureConfiguration<TDbContext> UseDefaultProviderAttribute(DatabaseProviderAttribute attribute)
    {
        DefaultProviderAttribute = attribute;
        return this;
    }

    /// <summary> Can return another attribute (and thus DB provider) than detected. Use this for example to replace an InMemory-Provider with a DB-Provider during nightly runs. </summary>
    public DatabaseFixtureConfiguration<TDbContext> UseProviderAttributeReplacer(Func<DatabaseProviderAttribute, DatabaseProviderAttribute> replaceProvider)
    {
        ReplaceProvider = replaceProvider;
        return this;
    }

    /// <summary>
    /// XUnit limits the number of maximum <i>active</i> tests executing, but it does not the limit of maximum parallel tests started.
    /// As soon as a test awaits a task somewhere, the thread is returned to the pool and another test gets started. This is intended by design.<br/>
    /// This behavior can cause issues when running integration tests against a database, especially when lots of tests are started.
    /// Connection limits can be exhausted quickly and other issues, like timeouts due to overload, may occur.<br/>
    /// This setting limits maximum concurrent database tests that can be started.<br/>
    /// Note that this setting is not affecting the connection limit of entity framework or any other connection limits.
    /// Entity framework or this unit testing framework could still have more open connections than the maxConcurrency setting, but it still can be
    /// leveraged to drastically reduce the chance of connection limit exhaustion and timeouts due to a too high load. <b/>
    /// </summary>
    /// <param name="maxConcurrency">
    /// &lt; 0: Disable limits<br/>
    /// 0: Use number of virtual CPUs available (default)<br/>
    /// &gt; 0: Use the given value as limit
    /// </param>
    [Obsolete("This method is obsolete and will be removed in v7.0. The maximum number of tests can now be set in the attribute 'FusonicTestFramework' on assembly level.")]
    public DatabaseFixtureConfiguration<TDbContext> SetMaxTestConcurrency(int maxConcurrency)
    {
        LimitTestConcurrencyAttribute.MaxConcurrency = maxConcurrency;
        return this;
    }
}
