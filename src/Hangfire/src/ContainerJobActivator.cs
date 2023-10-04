// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Hangfire;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.Extensions.Hangfire;

public sealed class ContainerJobActivator(Container container) : JobActivator
{
    public override object ActivateJob(Type jobType)
        => container.GetInstance(jobType);

    public override JobActivatorScope BeginScope(JobActivatorContext context)
        => new SimpleInjectorScope(container);

    private sealed class SimpleInjectorScope(Container container) : JobActivatorScope
    {
        private readonly Scope scope = AsyncScopedLifestyle.BeginScope(container);

        public override object Resolve(Type type)
            => scope.Container!.GetInstance(type);

        public override void DisposeScope()
        {
            base.DisposeScope();
            scope.DisposeScopeAsync().Wait();
        }
    }
}
