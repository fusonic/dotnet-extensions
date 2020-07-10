using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Fusonic.Extensions.UnitTests;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using SimpleInjector;

namespace Fusonic.Extensions.Email.Tests
{
    public class TestFixture : UnitTestFixture
    {
        protected override void RegisterCoreDependencies(Container container)
        {
            base.RegisterCoreDependencies(container);

            container.RegisterEmail(o =>
            {
                var path = Path.GetDirectoryName(typeof(TestFixture).Assembly.Location)!;

                o.SenderAddress = "test@fusonic.net";
                o.SenderName = "Test";
                o.CssPath = Path.Combine(path, "email.css");
                o.StoreInDirectory = path;
            });

            container.Register<RazorEmailRenderingService>();

            container.RegisterInstance<Func<IViewLocalizer>>(() => Substitute.For<IViewLocalizer>());

            var mediatorAssemblies = new[] { typeof(IMediator).Assembly, typeof(SendEmail).Assembly };
            container.Register(() => new ServiceFactory(container.GetInstance), Lifestyle.Singleton);
            container.RegisterSingleton<IMediator, Mediator>();
            container.Register(typeof(IRequestHandler<,>), mediatorAssemblies);

            container.Collection.Register(typeof(INotificationHandler<>), mediatorAssemblies);
            container.Collection.Register(typeof(IPipelineBehavior<,>),
                new[]
                {
                    typeof(RequestPreProcessorBehavior<,>),
                    typeof(RequestPostProcessorBehavior<,>)
                });

            container.Collection.Register(typeof(IRequestPreProcessor<>), mediatorAssemblies);
            container.Collection.Register(typeof(IRequestPostProcessor<,>), mediatorAssemblies);

            var viewLocalizer = Substitute.For<IViewLocalizer>();
            viewLocalizer.GetString(null).ReturnsForAnyArgs(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
            container.RegisterInstance(viewLocalizer);

            var services = new ServiceCollection();

            services.AddSimpleInjector(container, setup => setup.AutoCrossWireFrameworkComponents = true);

            var viewsDll = typeof(TestFixture).Assembly.GetName().Name + ".Views.dll";
            var viewsAssembly = Assembly.LoadFrom(viewsDll);

            services.AddRazorPages()
                    .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(viewsAssembly)));

            services.AddLogging();

            var listener = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton(listener);
            services.AddSingleton<DiagnosticSource>(listener);

            services.BuildServiceProvider(validateScopes: true).UseSimpleInjector(container);
        }
    }
}