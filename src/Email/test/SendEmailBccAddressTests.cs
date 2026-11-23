// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Fusonic.Extensions.Email.Tests.Models;
using SimpleInjector;

namespace Fusonic.Extensions.Email.Tests;

[Collection("Mailpit")]
public class SendEmailBccAddressTests(SendEmailBccAddressTests.SendEmailBccAddressTestsFixture fixture) : TestBase<SendEmailBccAddressTests.SendEmailBccAddressTestsFixture>(fixture)
{
    [Fact]
    public async Task SendEmail_NoBccDefined_SetsConfiguredBccAddress()
    {
        await Mailpit.DeleteEmailsAsync();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        email.Message.To.Mailboxes.Select(a => a.Address).Should().BeEquivalentTo(["recipient@fusonic.net"]);
        email.BccAddresses.Should().BeEquivalentTo(["bcc@fusonic.net"]);
    }

    [Fact]
    public async Task SendEmail_BccDefined_ConfiguredBccAddressDoesNotOverwrite()
    {
        await Mailpit.DeleteEmailsAsync();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, BccRecipient: "franz@fusonic.net"));

        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        email.Message.To.Mailboxes.Select(a => a.Address).Should().BeEquivalentTo(["recipient@fusonic.net"]);
        email.BccAddresses.Should().BeEquivalentTo(["franz@fusonic.net"]);
    }

    public class SendEmailBccAddressTestsFixture : TestFixture
    {
        protected override void RegisterDependencies(Container container)
        {
            base.RegisterDependencies(container);

            container.RegisterEmail(c =>
            {
                c.CssPath = "email.css";
                c.SmtpServer = TestStartup.SmtpHost;
                c.SmtpPort = TestStartup.SmtpPort;
                c.SenderAddress = "test@fusonic.net";
                c.SenderName = "Test Fusonic";
                c.BccAddress = "bcc@fusonic.net";
            });
        }
    }
}