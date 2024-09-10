// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using FluentAssertions;
using Fusonic.Extensions.Email.Tests.Models;
using netDumbster.smtp;
using SimpleInjector;
using Xunit;

namespace Fusonic.Extensions.Email.Tests;

public class SendEmailBccDecoratorTests(SendEmailBccDecoratorTests.SendEmailBccDecoratorTestsFixture fixture) : TestBase<SendEmailBccDecoratorTests.SendEmailBccDecoratorTestsFixture>(fixture)
{
    [Fact]
    public async Task SendEmail_NoBccDefined_SetsConfiguredBccAddress()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.ToAddresses
             .Should()
             .HaveCount(2)
             .And.Contain(a => a.Address == "recipient@fusonic.net")
             .And.Contain(a => a.Address == "bcc@fusonic.net");
    }

    [Fact]
    public async Task SendEmail_BccDefined_ConfiguredBccAddressDoesNotOverwrite()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, BccRecipient: "franz@fusonic.net"));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.ToAddresses
             .Should()
             .HaveCount(2)
             .And.Contain(a => a.Address == "recipient@fusonic.net")
             .And.Contain(a => a.Address == "franz@fusonic.net");
    }

    public class SendEmailBccDecoratorTestsFixture : TestFixture
    {
        public SimpleSmtpServer? SmtpServer { get; private set; }

        protected override void RegisterDependencies(Container container)
        {
            base.RegisterDependencies(container);

            SmtpServer = SimpleSmtpServer.Start();

            container.RegisterEmail(c =>
            {
                c.CssPath = "email.css";
                c.SmtpServer = "localhost";
                c.SmtpPort = SmtpServer.Configuration.Port;
                c.SenderAddress = "test@fusonic.net";
                c.SenderName = "Test Fusonic";
                c.BccAddress = "bcc@fusonic.net";
            });
        }

        public override async ValueTask DisposeAsync()
        {
            SmtpServer?.Dispose();
            await base.DisposeAsync();
        }
    }
}