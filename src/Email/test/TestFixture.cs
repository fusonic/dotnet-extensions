// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using Fusonic.Extensions.UnitTests.SimpleInjector;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SimpleInjector;

namespace Fusonic.Extensions.Email.Tests;

public class TestFixture : SimpleInjectorTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
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

        var viewLocalizer = Substitute.For<IViewLocalizer>();
        container.RegisterInstance<Func<IViewLocalizer>>(() => viewLocalizer);

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

        var services = new ServiceCollection();

        services.AddSimpleInjector(container, setup => setup.AutoCrossWireFrameworkComponents = true);

        services.AddRazorPages()
                .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(Assembly.GetExecutingAssembly())));

        services.AddLogging();

        var listener = new DiagnosticListener("Microsoft.AspNetCore");
        services.AddSingleton(listener);
        services.AddSingleton<DiagnosticSource>(listener);

        services.BuildServiceProvider(validateScopes: true).UseSimpleInjector(container);
    }
}
