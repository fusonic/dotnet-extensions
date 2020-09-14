using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Fusonic.Extensions.UnitTests.XunitExtensibility;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.Extensions.UnitTests
{
    public abstract class UnitTestFixture : IDisposable
    {
        private readonly Container container = new Container();

        protected virtual bool VerifyContainer { get; } = true;

        public IConfiguration Configuration { get; }

        protected UnitTestFixture()
        {
            Configuration = BuildConfiguration();
            ConfigureContainer();
        }

        public static IConfiguration GetDefaultConfiguration(string basePath, Assembly assembly)
        {
            return new ConfigurationBuilder()
                  .SetBasePath(basePath)
                  .AddJsonFile("testsettings.json", optional: true)
                  .AddUserSecrets(assembly, optional: true)
                  .AddEnvironmentVariables()
                  .Build();
        }

        protected virtual IConfiguration BuildConfiguration() => GetDefaultConfiguration(Directory.GetCurrentDirectory(), GetType().Assembly);

        private void ConfigureContainer()
        {
            if (GetType().Assembly.GetCustomAttribute<FusonicTestFrameworkAttribute>() == null)
                throw new InvalidOperationException($"{nameof(FusonicTestFrameworkAttribute)} must be set in test assembly '{GetType().Assembly.FullName}'.");

            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Options.AllowOverridingRegistrations = true;

            RegisterCoreDependencies(container);
            RegisterDependencies(container);

            if (VerifyContainer)
                container.Verify();
        }

        protected virtual void RegisterCoreDependencies(Container container)
        { }

        protected virtual void RegisterDependencies(Container container)
        { }

        [DebuggerStepThrough]
        public Scope BeginLifetimeScope()
        {
            return AsyncScopedLifestyle.BeginScope(container);
        }

        public virtual void Dispose()
        {
            container.Dispose();
        }
    }
}