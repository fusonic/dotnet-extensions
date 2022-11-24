// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.SimpleInjector;
using Fusonic.Extensions.XUnit.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleInjector;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore;

public static class SimpleInjectorExtensions
{
    /// <summary>
    /// Registers a DbContext where each test gets an own database. Note: You need to additionally register an <see cref="ITestDatabaseProvider"/> to support your specific database.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the EF Core DbContext to register</typeparam>
    /// <typeparam name="TTestDatabaseProvider">The type of the ITestDatabaseProvider responsible for creating and dropping a test database within a single test.</typeparam>
    /// <param name="container">The simple injector container.</param>
    /// <param name="configureBuilder">Configure the DbContextOptionsBuilder. Parameters are the name of the test database and the builder to configure.</param>
    /// <param name="enableDbContextLogging">If enabled, the dbContext logs to the test output. Can be helpful for debugging. Disabled by default.</param>
    public static void RegisterDbContext<TDbContext, TTestDatabaseProvider>(this Container container,
        Action<string, DbContextOptionsBuilder<TDbContext>> configureBuilder,
        bool enableDbContextLogging = false)
        where TDbContext : DbContext
        where TTestDatabaseProvider : class, ITestDatabaseProvider
    {
        container.RegisterTestScoped<TTestDatabaseProvider>();

        container.RegisterTestScoped(() =>
        {
            var dbProvider = container.GetInstance<TTestDatabaseProvider>();
            var dbName = dbProvider.TestDbName;

            var builder = new DbContextOptionsBuilder<TDbContext>();
            configureBuilder(dbName, builder);

            if (enableDbContextLogging)
                builder.UseLoggerFactory(new LoggerFactory(new[] { new XUnitLoggerProvider() }));

            builder.AddInterceptors(new CreateDatabaseInterceptor<TDbContext>(ctx => dbProvider.CreateDatabase(ctx)));

            return builder.Options;
        });

        container.Register<TDbContext>(Lifestyle.Scoped);
    }
}