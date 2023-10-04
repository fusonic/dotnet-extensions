// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.UnitTests.ServiceProvider;

public abstract class ServiceProviderTestFixture : DependencyInjectionTestFixture<AsyncServiceScope>, IDisposable
{
    private readonly Microsoft.Extensions.DependencyInjection.ServiceProvider serviceProvider;

    protected virtual ServiceProviderOptions ServiceProviderOptions { get; } = new()
    {
        ValidateOnBuild = true,
        ValidateScopes = true
    };

    protected ServiceProviderTestFixture() => serviceProvider = BuildServiceProvider();

    [DebuggerStepThrough]
    public sealed override AsyncServiceScope BeginScope() => serviceProvider.CreateAsyncScope();

    [DebuggerStepThrough]
    public sealed override T GetInstance<T>(AsyncServiceScope scope) => scope.ServiceProvider.GetRequiredService<T>();

    [DebuggerStepThrough]
    public override object GetInstance(AsyncServiceScope scope, Type serviceType) => scope.ServiceProvider.GetRequiredService(serviceType);

    private Microsoft.Extensions.DependencyInjection.ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        RegisterCoreDependencies(services);
        RegisterDependencies(services);

        return services.BuildServiceProvider(ServiceProviderOptions);
    }

    protected virtual void RegisterCoreDependencies(IServiceCollection services) { }

    protected virtual void RegisterDependencies(IServiceCollection services) { }

    public void Dispose()
    {
        if (serviceProvider is IDisposable d)
            d.Dispose();

        GC.SuppressFinalize(this);
    }
}
