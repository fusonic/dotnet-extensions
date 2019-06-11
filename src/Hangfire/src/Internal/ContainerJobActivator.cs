using System;
using Hangfire;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.Extensions.Hangfire.Internal
{
    public sealed class ContainerJobActivator : JobActivator
    {
        private readonly Container container;

        public ContainerJobActivator(Container container)
            => this.container = container;

        public override object ActivateJob(Type type)
            => container.GetInstance(type);

        public override JobActivatorScope BeginScope(JobActivatorContext context)
            => new SimpleInjectorScope(container);

        private sealed class SimpleInjectorScope : JobActivatorScope
        {
            private readonly Scope scope;

            public SimpleInjectorScope(Container container)
                => scope = AsyncScopedLifestyle.BeginScope(container);

            public override object Resolve(Type type)
                => scope.Container.GetInstance(type);

            public override void DisposeScope()
            {
                base.DisposeScope();
                scope.Dispose();
            }
        }
    }
}