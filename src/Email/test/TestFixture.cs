// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Fusonic.Extensions.Mediator;
using Fusonic.Extensions.UnitTests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var services = new ServiceCollection();
        container.RegisterMediator(services, [typeof(SendEmail).Assembly]);

        services.AddRazorComponents();
        services.AddRazorPages()
                .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(Assembly.GetExecutingAssembly())));

        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new List<CultureInfo>
            {
                new("de-CH"),
                new("en-CH"),
                new("fr-CH"),
                new("it-CH"),
            };

            options.DefaultRequestCulture = new RequestCulture("en");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build());
        services.AddScoped<HtmlRenderer>();

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