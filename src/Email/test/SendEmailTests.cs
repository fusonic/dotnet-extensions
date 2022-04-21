// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using Fusonic.Extensions.Email.Tests.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using netDumbster.smtp;
using NSubstitute;
using SimpleInjector;
using Xunit;

namespace Fusonic.Extensions.Email.Tests;

public class SendEmailTests : TestBase<SendEmailTests.SendEmailFixture>
{
    public SendEmailTests(SendEmailFixture fixture) : base(fixture)
    { }

    [Fact]
    public async Task SendEmail()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new RenderTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail.Command("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.ToAddresses
             .Should().HaveCount(1)
             .And.Contain(a => a.Address == "recipient@fusonic.net");

        email.FromAddress.Address.Should().Be("test@fusonic.net");
        email.Headers.AllKeys.Should().Contain("Subject");
        email.Headers["Subject"].Should().Be("The subject");

        email.MessageParts.Should().HaveCount(1);
        email.MessageParts[0].BodyData.Should().Be(
            "<html><head><title>Render Test</title></head>" + Environment.NewLine
          + "<body style=\"color: red\"><p>Some field.</p>" + Environment.NewLine
          + "</body></html>");
    }

    [Fact]
    public async Task SendEmail_BccAdded()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new RenderTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail.Command("recipient@fusonic.net", "The Recipient", "bcc@fusonic.net", new CultureInfo("de-AT"), null, model));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.ToAddresses
             .Should().HaveCount(2)
             .And.Contain(a => a.Address == "recipient@fusonic.net")
             .And.Contain(a => a.Address == "bcc@fusonic.net");

        email.FromAddress.Address.Should().Be("test@fusonic.net");
        email.Headers.AllKeys.Should().Contain("Subject");
        email.Headers["Subject"].Should().Be("The subject");

        email.MessageParts.Should().HaveCount(1);
        email.MessageParts[0].BodyData.Should().Be(
            "<html><head><title>Render Test</title></head>" + Environment.NewLine
          + "<body style=\"color: red\"><p>Some field.</p>" + Environment.NewLine
          + "</body></html>");
    }

    [Theory]
    [InlineData("testFile.txt")]
    [InlineData("speciölChäracters.xml")]
    [InlineData("!even-More.xml")]
    public async Task SendEmail_AttachmentsAdded(string attachmentName)
    {
        var attachmentPath = Path.Combine(Path.GetDirectoryName(typeof(TestFixture).Assembly.Location)!, "email.css");
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new RenderTestEmailViewModel { SomeField = "Some field." };
        var attachments = new Attachment[]
        {
            new(
                attachmentName,
                new Uri(attachmentPath)
            )
        };

        await SendAsync(new SendEmail.Command("recipient@fusonic.net", "The Recipient", attachments, new CultureInfo("de-AT"), model));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.MessageParts.Should().HaveCount(2);

        var onlyAsciiChars = new Regex("^[a-zA-Z0-9.!-]*$");
        if (!onlyAsciiChars.IsMatch(attachmentName))
        {
            var base64EncodedName = Convert.ToBase64String(Encoding.UTF8.GetBytes(attachmentName));
            email.MessageParts[1].HeaderData.Should().Be($"application/octet-stream; name=\"=?utf-8?B?{base64EncodedName}?=\"");
        }
        else
        {
            email.MessageParts[1].HeaderData.Should().Contain(attachmentName);
        }

        var expectedAttachmentContent = await File.ReadAllTextAsync(attachmentPath);
        email.MessageParts[1].BodyData.Should().Be(expectedAttachmentContent);
    }

    [Fact]
    public async Task SendEmail_InvalidEmailAddress_ThrowsException()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new RenderTestEmailViewModel { SomeField = "Some field." };

        Func<Task> act = async () => await SendAsync(new SendEmail.Command("recipient@fusonic.net", "The Recipient", "invalidEmailAddress", new CultureInfo("de-AT"), null, null, model));

        await act.Should().ThrowAsync<Exception>();
    }

    public class SendEmailFixture : TestFixture
    {
        public SimpleSmtpServer? SmtpServer { get; private set; }

        protected override void RegisterDependencies(Container container)
        {
            base.RegisterDependencies(container);

            var localizer = Substitute.For<IViewLocalizer>();
            localizer.GetString(Arg.Any<string>()).ReturnsForAnyArgs(_ => new LocalizedString("Subject", "The subject"));
            container.RegisterInstance(localizer);

            SmtpServer = SimpleSmtpServer.Start();

            container.RegisterEmail(c =>
            {
                c.CssPath = "email.css";
                c.SmtpServer = "localhost";
                c.SmtpPort = SmtpServer.Configuration.Port;
                c.SenderAddress = "test@fusonic.net";
                c.SenderName = "Test Fusonic";
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            SmtpServer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
