using System;
using SimpleInjector;

namespace Fusonic.Extensions.Email
{
    public static class ContainerExtensions
    {
        /// <summary> Registers the components for sending emails. </summary>
        public static void RegisterEmail(this Container container, Action<EmailOptions> configure)
        {
            var options = new EmailOptions();
            configure(options);
            options.Validate();

            container.RegisterInstance(options);
            container.RegisterInstance(new CssInliner(options));
            container.Register<IEmailRenderingService, RazorEmailRenderingService>();
            container.Register<ISmtpClient, SmtpClient>();
        }
    }
}