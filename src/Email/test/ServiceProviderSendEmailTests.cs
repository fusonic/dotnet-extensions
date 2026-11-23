// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using System.Globalization;
using Fusonic.Extensions.Email.Tests.Models;
using Fusonic.Extensions.Mediator;
using Fusonic.Extensions.Mediator.DependencyInjection;
using Fusonic.Extensions.UnitTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using netDumbster.smtp;

namespace Fusonic.Extensions.Email.Tests;

/// <summary> Tests the registration with Microsoft.Extensions.DependencyInjection. </summary>
public class ServiceProviderSendEmailTests(ServiceProviderSendEmailTests.ServiceProviderFixture fixture) : DependencyInjectionUnitTest<ServiceProviderSendEmailTests.ServiceProviderFixture>(fixture)
{
    [Fact]
    public async Task SendEmail_RazorView_IsSentWithBcc()
    {
        Fixture.SmtpServer.ClearReceivedEmail();

        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), new SendEmailTestEmailViewModel { SomeField = "Some field." }));

        var email = Fixture.SmtpServer.ReceivedEmail.Should().ContainSingle().Subject;
        email.ToAddresses.Select(a => a.Address).Should().BeEquivalentTo(["recipient@fusonic.net"]);
        email.BccAddresses.Select(a => a.Address).Should().BeEquivalentTo(["bcc@fusonic.net"]);
        email.MessageParts[0].BodyData.Should().Contain("Some field.");
    }

    [Fact]
    public async Task SendEmail_BlazorComponent_IsSent()
    {
        Fixture.SmtpServer.ClearReceivedEmail();

        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), new SendEmailTestEmailComponentModel { SomeField = "Some field." }));

        var email = Fixture.SmtpServer.ReceivedEmail.Should().ContainSingle().Subject;
        email.MessageParts[0].BodyData.Should().Contain("Some field.");
    }

    [Fact]
    public void AddEmail_WithBccAddress_DoesNotDecorateHandler()
    {
        // Decorated handlers are hidden from the Hangfire out-of-band decorators.
        var services = new ServiceCollection();

        services.AddEmail(c =>
        {
            c.SmtpServer = "localhost";
            c.SenderAddress = "test@fusonic.net";
            c.BccAddress = "bcc@fusonic.net";
        });

        services.Should().ContainSingle(d => d.ServiceType == typeof(IRequestHandler<SendEmail, Unit>))
                .Which.ImplementationType.Should().Be<SendEmail.Handler>();
    }

    [DebuggerStepThrough]
    private Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => ScopedAsync(() => GetInstance<IMediator>().Send(request));

    public class ServiceProviderFixture : ServiceProviderTestFixture, IDisposable
    {
        public SimpleSmtpServer SmtpServer { get; } = SimpleSmtpServer.Start();

        protected override void RegisterCoreDependencies(IServiceCollection services)
        {
            TestFixture.AddFrameworkServices(services);
            services.AddSingleton<IHostEnvironment>(sp => sp.GetRequiredService<IWebHostEnvironment>());
            services.AddScoped<IMediator>(sp => new ServiceProviderMediator(sp));

            services.AddEmail(c =>
            {
                c.CssPath = "email.css";
                c.SmtpServer = "localhost";
                c.SmtpPort = SmtpServer.Configuration.Port;
                c.SenderAddress = "test@fusonic.net";
                c.SenderName = "Test Fusonic";
                c.BccAddress = "bcc@fusonic.net";
            });
        }

        void IDisposable.Dispose()
        {
            SmtpServer.Dispose();
            Dispose();
        }
    }
}
