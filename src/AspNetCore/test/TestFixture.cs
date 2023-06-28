// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using Fusonic.Extensions.AspNetCore.Razor;
using Fusonic.Extensions.UnitTests.SimpleInjector;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Fusonic.Extensions.AspNetCore.Tests;

public class TestFixture : SimpleInjectorTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        base.RegisterCoreDependencies(container);

        container.Register<RazorViewRenderingService>();

        var services = new ServiceCollection();
        services.AddRazorPages()
                .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(Assembly.GetExecutingAssembly())));

        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddScoped<IViewLocalizer, ViewLocalizer>();
        services.AddScoped<IHtmlLocalizerFactory, HtmlLocalizerFactory>();
        services.AddSingleton<IWebHostEnvironment, TestWebHostEnvironment>();

        var listener = new DiagnosticListener("Microsoft.AspNetCore");
        services.AddSingleton(listener);
        services.AddSingleton<DiagnosticSource>(listener);

        services.AddSimpleInjector(container, setup => setup.AutoCrossWireFrameworkComponents = true);
        services.BuildServiceProvider(validateScopes: true).UseSimpleInjector(container);
    }
}