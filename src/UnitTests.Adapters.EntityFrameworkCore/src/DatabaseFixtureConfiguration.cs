// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    public class DatabaseFixtureConfiguration<TDbContext>
        where TDbContext : DbContext
    {
        internal Dictionary<Type, Func<DatabaseProviderAttribute, ITestDatabaseProvider<TDbContext>>> ProviderFactories { get; } = new Dictionary<Type, Func<DatabaseProviderAttribute, ITestDatabaseProvider<TDbContext>>>();
        internal DatabaseProviderAttribute? DefaultProviderAttribute { get; private set; }
        internal Func<DatabaseProviderAttribute, DatabaseProviderAttribute>? ReplaceProvider { get; private set; }

        public DatabaseFixtureConfiguration()
        {
            ProviderFactories[typeof(NoDatabaseAttribute)] = attr => new NoDatabaseProvider<TDbContext>();
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
    }
}