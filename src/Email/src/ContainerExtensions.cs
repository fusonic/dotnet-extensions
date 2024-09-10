// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Fusonic.Extensions.AspNetCore.Blazor;
using Fusonic.Extensions.AspNetCore.Razor;
using Fusonic.Extensions.Mediator;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using SimpleInjector;

namespace Fusonic.Extensions.Email;

public static class ContainerExtensions
{
    /// <summary>
    /// Registers the components for sending emails.
    ///
    /// Note: You need to register ViewLocalization as well as the HtmlRenderer in your services.
    /// Example:
    /// <code>
    /// services.AddScoped{HtmlRenderer}(); 
    /// services.AddLocalization(options => options.ResourcesPath = "Resources/Localization");
    ///
    /// services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)))
    ///         .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
    /// </code>
    /// </summary>
    public static void RegisterEmail(this Container container, Action<EmailOptions> configure)
    {
        var options = new EmailOptions();
        configure(options);
        options.Validate();

        container.RegisterInstance(options);

        container.RegisterInstance(new CssInliner(options));

        container.RegisterInstance(container.GetInstance<IViewLocalizer>);
        container.RegisterInstance(container.GetInstance<IStringLocalizerFactory>);

        container.Register<IRazorViewRenderingService, RazorViewRenderingService>();
        container.Register<IBlazorRenderingService, BlazorRenderingService>();

        container.Collection.Append<IEmailRenderingService, RazorEmailRenderingService>();
        container.Collection.Append<IEmailRenderingService, BlazorEmailRenderingService>();

        container.Register<ISmtpClient, SmtpClient>();
        container.Collection.Append<IEmailAttachmentResolver, FileAttachmentResolver>(Lifestyle.Singleton);

        container.Register(typeof(IRequestHandler<,>), Assembly.GetExecutingAssembly());

        if (!string.IsNullOrWhiteSpace(options.BccAddress))
        {
            container.RegisterInstance(new SendEmailBccDecorator.Options(options.BccAddress));
            container.RegisterDecorator<IRequestHandler<SendEmail, Unit>, SendEmailBccDecorator>();
        }
    }
}