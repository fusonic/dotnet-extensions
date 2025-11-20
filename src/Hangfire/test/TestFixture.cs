// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Common.Security;
using Fusonic.Extensions.Mediator;
using Fusonic.Extensions.UnitTests;
using Fusonic.Extensions.UnitTests.EntityFrameworkCore;
using Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SimpleInjector;

namespace Fusonic.Extensions.Hangfire.Tests;

public class TestFixture : SimpleInjectorTestFixture
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;
    public Container Container { get; private set; } = null!;

    protected sealed override void RegisterCoreDependencies(Container container)
    {
        var services = new ServiceCollection();

        RegisterMediator(container, services);

        container.RegisterOutOfBandDecorators();

        container.RegisterInstance(Substitute.For<IUserAccessor>());

        RegisterDatabase(services);
        RegisterHangfire(services);
        services.AddSimpleInjector(container, setup => setup.AutoCrossWireFrameworkComponents = true);

        ServiceProvider = services.BuildServiceProvider(validateScopes: true).UseSimpleInjector(container);
        Container = container;
    }

    private static void RegisterMediator(Container container, IServiceCollection services)
    {
        container.RegisterMediator(services, [typeof(TestFixture).Assembly]);
    }

    private void RegisterDatabase(IServiceCollection services)
    {
        // Database
        var testStore = new NpgsqlDatabasePerTestStore(TestStartup.ConnectionString);
        services.AddSingleton<ITestStore>(testStore);
        services.AddDbContext<TestDbContext>(b => b.UseNpgsqlDatabasePerTest(testStore));
    }

    protected virtual void RegisterHangfire(IServiceCollection services) => RegisterHangfireMock(services);

    protected static void RegisterHangfireMock(IServiceCollection services)
        => services.AddSingleton(Substitute.For<IBackgroundJobClient>());

    protected void RegisterHangfireNpgsql(IServiceCollection services)
    {
        // Hangfire
        var connectionFactory = new HangfireTestConnectionFactory(() => ((NpgsqlDatabasePerTestStore)Container.GetInstance<ITestStore>()).ConnectionString);
        services.AddSingleton(connectionFactory);
        services.AddHangfire(c => c.UsePostgreSqlStorage(options => options.UseConnectionFactory(connectionFactory), new PostgreSqlStorageOptions
        {
            EnableTransactionScopeEnlistment = true,
            PrepareSchemaIfNecessary = false // If true, Hangfire runs its scripts directly here on the not yet existing DB.
        }));
    }
}