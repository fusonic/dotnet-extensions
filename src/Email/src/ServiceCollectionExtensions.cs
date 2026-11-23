// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.AspNetCore.Blazor;
using Fusonic.Extensions.AspNetCore.Razor;
using Fusonic.Extensions.Mediator;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fusonic.Extensions.Email;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the components for sending emails using Microsoft.Extensions.DependencyInjection.
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
    public static IServiceCollection AddEmail(this IServiceCollection services, Action<EmailOptions> configure)
    {
        var options = new EmailOptions();
        configure(options);
        options.Validate();

        services.AddSingleton(options);
        services.AddSingleton(new CssInliner(options));

        services.AddScoped<Func<IViewLocalizer>>(sp => sp.GetRequiredService<IViewLocalizer>);

        services.AddScoped<IRazorViewRenderingService>(sp => ActivatorUtilities.CreateInstance<RazorViewRenderingService>(sp, new CurrentServiceScope(sp)));
        services.AddScoped<IBlazorRenderingService, BlazorRenderingService>();

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IEmailRenderingService, RazorEmailRenderingService>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IEmailRenderingService, BlazorEmailRenderingService>());

        services.AddScoped<ISmtpClient, SmtpClient>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEmailAttachmentResolver, FileAttachmentResolver>());

        services.AddScoped<IRequestHandler<SendEmail, Unit>, SendEmail.Handler>();

        return services;
    }

    /// <summary> Provides the current scope to services depending on <see cref="IServiceScope"/>. The scope is owned by the caller and not disposed here. </summary>
    private sealed class CurrentServiceScope(IServiceProvider serviceProvider) : IServiceScope
    {
        public IServiceProvider ServiceProvider => serviceProvider;

        public void Dispose()
        { }
    }
}
