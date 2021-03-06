// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using Fusonic.Extensions.UnitTests.SimpleInjector;
using Microsoft.EntityFrameworkCore;
using SimpleInjector;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    public abstract class DatabaseFixture<TDbContext> : UnitTestFixture
        where TDbContext : DbContext
    {
        protected abstract void ConfigureDatabaseProviders(DatabaseFixtureConfiguration<TDbContext> configuration);

        protected override void RegisterCoreDependencies(Container container)
        {
            base.RegisterCoreDependencies(container);

            //Configuration for the database providers. Usually global, but may be overridden by specific test fixtures.
            container.Register(() =>
                {
                    var configuration = new DatabaseFixtureConfiguration<TDbContext>();
                    ConfigureDatabaseProviders(configuration);
                    return configuration;
                },
                Lifestyle.Singleton);

            //The database provider attribute used on the test
            container.RegisterTestScoped(() =>
            {
                if (container.IsVerifying)
                    return new NoDatabaseAttribute();

                var configuration = container.GetInstance<DatabaseFixtureConfiguration<TDbContext>>();
                var providerAttribute = DatabaseTestContext.CurrentProviderAttribute.Value ?? configuration.DefaultProviderAttribute;
                if (providerAttribute == null)
                    throw new InvalidOperationException("Could not determine database provider. No DatabaseProviderAttribute is set on the class or the method. No default attribute is configured.");

                if (configuration.ReplaceProvider != null)
                    providerAttribute = configuration.ReplaceProvider(providerAttribute);

                return providerAttribute;
            });

            //The database provider instance itself that manages db creation, seeding, deletion and the options injected into the dbContext.
            container.RegisterTestScoped(() =>
            {
                var configuration = container.GetInstance<DatabaseFixtureConfiguration<TDbContext>>();
                var attribute = container.GetInstance<DatabaseProviderAttribute>();

                var attributeType = attribute.GetType();
                if (!configuration!.ProviderFactories.ContainsKey(attributeType))
                    throw new InvalidOperationException($"Cannot resolve provider for attribute {attributeType.FullName}. No provider is registered for it.");

                var providerFactory = configuration.ProviderFactories[attributeType];
                var provider = providerFactory(attribute);
                return provider;
            });

            //The db context options given by the database provider
            container.RegisterTestScoped(() =>
            {
                var provider = container.GetInstance<ITestDatabaseProvider<TDbContext>>();
                return provider.GetContextOptions();
            });

            //The db context
            container.Register<TDbContext>(Lifestyle.Scoped);
        }
    }
}