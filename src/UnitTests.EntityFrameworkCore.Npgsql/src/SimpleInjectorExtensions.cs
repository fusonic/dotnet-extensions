// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.SimpleInjector;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using SimpleInjector;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

public static class SimpleInjectorExtensions
{
    /// <summary>
    /// Registers a DbContext where each test gets an own database. Note: You need to additionally register an <see cref="ITestDatabaseProvider"/> to support your specific database.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the EF Core DbContext to register</typeparam>
    /// <param name="container">The simple injector container.</param>
    /// <param name="configureSettings">Configure the settings for the provider, like the connection string to the template.</param>
    /// <param name="enableDbContextLogging">If enabled, the dbContext logs to the test output. Can be helpful for debugging. Disabled by default.</param>
    /// <param name="npgsqlOptions">An optional action to allow additional Npgsql-configuration.</param>
    /// <param name="dbContextBuilderOptions">An optional action to allow additional DbContextOptionsBuilder-configuration.</param>
    public static void RegisterNpgsqlDbContext<TDbContext>(
        this Container container,
        Action<NpgsqlTestDatabaseProviderSettings> configureSettings,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptions = null,
        Action<DbContextOptionsBuilder<TDbContext>>? dbContextBuilderOptions = null,
        bool enableDbContextLogging = false)
        where TDbContext : DbContext
        => container.RegisterNpgsqlDbContext<TDbContext, NpgsqlTestDatabaseProvider, NpgsqlTestDatabaseProviderSettings>(
            configureSettings,
            npgsqlOptions,
            dbContextBuilderOptions,
            enableDbContextLogging);

    /// <summary>
    /// Registers a DbContext where each test gets an own database. Note: You need to additionally register an <see cref="ITestDatabaseProvider"/> to support your specific database.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the EF Core DbContext to register</typeparam>
    /// <typeparam name="TProvider">The type of the test database provider</typeparam>
    /// <typeparam name="TSettings">The type of the settings for the test database provider</typeparam>
    /// <param name="container">The simple injector container.</param>
    /// <param name="configureSettings">Configure the settings for the provider, like the connection string to the template.</param>
    /// <param name="enableDbContextLogging">If enabled, the dbContext logs to the test output. Can be helpful for debugging. Disabled by default.</param>
    /// <param name="npgsqlOptions">An optional action to allow additional Npgsql-configuration.</param>
    /// <param name="dbContextBuilderOptions">An optional action to allow additional DbContextOptionsBuilder-configuration.</param>
    public static void RegisterNpgsqlDbContext<TDbContext, TProvider, TSettings>(
        this Container container,
        Action<TSettings> configureSettings,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptions = null,
        Action<DbContextOptionsBuilder<TDbContext>>? dbContextBuilderOptions = null,
        bool enableDbContextLogging = false)
        where TDbContext : DbContext
        where TProvider : NpgsqlTestDatabaseProvider
        where TSettings : NpgsqlTestDatabaseProviderSettings, new()
    {
        var settings = new TSettings();
        configureSettings(settings);

        container.RegisterInstance(settings);

        container.RegisterDbContext<TDbContext, TProvider>(
            (dbName, builder) =>
            {
                var connectionString = PostgreSqlUtil.ReplaceDatabaseName(settings.TemplateConnectionString, dbName);
                builder.UseNpgsql(connectionString, npgsqlOptions);
                dbContextBuilderOptions?.Invoke(builder);
            }, enableDbContextLogging);
    }
}