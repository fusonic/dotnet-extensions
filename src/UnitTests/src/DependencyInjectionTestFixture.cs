// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Configuration;

namespace Fusonic.Extensions.UnitTests;

public abstract class DependencyInjectionTestFixture<TScope> : IDependencyInjectionTestFixture
where TScope : notnull
{
    private static IConfiguration? defaultConfiguration;
    public IConfiguration Configuration { get; }

    protected DependencyInjectionTestFixture() => Configuration = BuildConfiguration();

    protected virtual IConfiguration BuildConfiguration()
        => defaultConfiguration
            ??= TestConfigurationHelper.GetDefaultConfiguration(Directory.GetCurrentDirectory(), GetType().Assembly);

    public abstract TScope BeginScope();
    public abstract object GetInstance(TScope scope, Type serviceType);
    public abstract T GetInstance<T>(TScope scope) where T : class;

    object IDependencyInjectionTestFixture.BeginScope() => BeginScope();
    object IDependencyInjectionTestFixture.GetInstance(object scope, Type serviceType) => GetInstance((TScope)scope, serviceType);
    T IDependencyInjectionTestFixture.GetInstance<T>(object scope) => GetInstance<T>((TScope)scope);
}