// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.UnitTests.ServiceProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests;

public class TestFixture : ServiceProviderTestFixture
{
    protected override void RegisterCoreDependencies(IServiceCollection services)
    {
        var testStore = new SqliteTestStore();
        services.AddSingleton<ITestStore>(testStore);

        services.AddDbContext<TestDbContext>(b => b.UseSqlite(testStore.ConnectionString)
                                                   .AddInterceptors(new ConnectionOpeningInterceptor(testStore.CreateDatabase)));
    }
}