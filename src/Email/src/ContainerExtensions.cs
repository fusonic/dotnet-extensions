using System;
using Microsoft.AspNetCore.Mvc.Localization;
using SimpleInjector;

namespace Fusonic.Extensions.Email
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Registers the components for sending emails.
        ///
        /// Note: You need to register ViewLocalization in your services.
        /// Example:
        /// <code>
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
            container.Register<IEmailRenderingService, RazorEmailRenderingService>();
            container.Register<ISmtpClient, SmtpClient>();
            container.RegisterInstance<Func<IViewLocalizer>>(container.GetInstance<IViewLocalizer>);
        }
    }
}