// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.Extensions.UnitTests.SimpleInjector;

public abstract class SimpleInjectorTestFixture : DependencyInjectionTestFixture<Scope>, IAsyncDisposable
{
    private readonly Container container = new();
    protected virtual bool VerifyContainer => true;

    protected SimpleInjectorTestFixture() => ConfigureContainer();

    private void ConfigureContainer()
    {
        container.Options.DefaultLifestyle = Lifestyle.Scoped;
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.Options.AllowOverridingRegistrations = true;

        RegisterCoreDependencies(container);
        RegisterDependencies(container);

        if (VerifyContainer)
            container.Verify();
    }

    protected virtual void RegisterCoreDependencies(Container container) { }

    protected virtual void RegisterDependencies(Container container) { }

    [DebuggerStepThrough]
    public sealed override Scope BeginScope() => AsyncScopedLifestyle.BeginScope(container);

    [DebuggerStepThrough]
    public sealed override T GetInstance<T>(Scope scope) => scope.Container!.GetInstance<T>();

    [DebuggerStepThrough]
    public override object GetInstance(Scope scope, Type serviceType) => scope.Container!.GetInstance(serviceType);

    // TODO: Remove with v9
    public virtual void Dispose()
    { }

    public virtual async ValueTask DisposeAsync()
    {
        Dispose();
        await container.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}