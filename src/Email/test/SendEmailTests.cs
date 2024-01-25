// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using Fusonic.Extensions.Email.Tests.Models;
using MailKit.Net.Smtp;
using netDumbster.smtp;
using SimpleInjector;
using Xunit;

namespace Fusonic.Extensions.Email.Tests;

public partial class SendEmailTests : TestBase<SendEmailTests.SendEmailFixture>
{
    public SendEmailTests(SendEmailFixture fixture) : base(fixture)
    { }

    [Fact]
    public async Task SendEmail()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.ToAddresses
             .Should()
             .HaveCount(1)
             .And.Contain(a => a.Address == "recipient@fusonic.net");

        email.FromAddress.Address.Should().Be("test@fusonic.net");
        email.Headers.AllKeys.Should().Contain("Subject");
        email.Headers["Subject"].Should().Be("The subject");

        email.MessageParts.Should().HaveCount(1);
        email.MessageParts[0]
             .BodyData.Should()
             .Be(
                  "<html><head><title>Render Test</title></head>" + Environment.NewLine
                                                                  + "<body style=\"color: red\"><p>Some field.</p>" + Environment.NewLine
                                                                  + "</body></html>");
    }

    [Fact]
    public async Task SendEmail_BccAdded()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, BccRecipient: "bcc@fusonic.net"));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.ToAddresses
             .Should()
             .HaveCount(2)
             .And.Contain(a => a.Address == "recipient@fusonic.net")
             .And.Contain(a => a.Address == "bcc@fusonic.net");

        email.FromAddress.Address.Should().Be("test@fusonic.net");
        email.Headers.AllKeys.Should().Contain("Subject");
        email.Headers["Subject"].Should().Be("The subject");

        email.MessageParts.Should().HaveCount(1);
        email.MessageParts[0]
             .BodyData.Should()
             .Be(
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

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        var attachments = new Attachment[]
        {
            new(
                attachmentName,
                new Uri(attachmentPath)
            )
        };

        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Attachments: attachments));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.MessageParts.Should().HaveCount(2);

        if (!GetAsciiCharsRegex().IsMatch(attachmentName))
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
    public async Task SendEmail_HeadersAdded()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Headers: new Dictionary<string, string> { ["my-header"] = "value" }));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();

        email.Headers.AllKeys.Should().Contain("my-header");
        email.Headers["my-header"].Should().Be("value");
    }

    [Fact]
    public async Task SendEmail_DefaultHeadersAdded()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var options = GetInstance<EmailOptions>();

        options.DefaultHeaders = new Dictionary<string, string> { ["my-header"] = "value" };

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();

        email.Headers.AllKeys.Should().Contain("my-header");
        email.Headers["my-header"].Should().Be("value");
    }

    [Fact]
    public async Task SendEmail_DefaultHeadersOverriddenIfSetTwice()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var options = GetInstance<EmailOptions>();

        options.DefaultHeaders = new Dictionary<string, string> { ["replaced"] = "value", ["default"] = "value" };

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Headers: new Dictionary<string, string> { ["replaced"] = "new-value" }));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();

        email.Headers.AllKeys.Should().Contain("replaced");
        email.Headers["replaced"].Should().Be("new-value");
        email.Headers.AllKeys.Should().Contain("default");
        email.Headers["default"].Should().Be("value");
    }

    [Fact]
    public async Task SendEmail_ReplyToAdded()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, ReplyTo: "reply@mail.com"));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();

        email.Headers.AllKeys.Should().Contain("Reply-To");
        email.Headers["Reply-To"].Should().Be("reply@mail.com");
    }

    [Fact]
    public async Task SendEmail_InvalidBccEmailAddress_ThrowsException()
    {
        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        Func<Task> act = async () => await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, BccRecipient: "invalidEmailAddress"));

        await act.Should().ThrowAsync<SmtpCommandException>();
    }

    [Fact]
    public async Task SendEmail_UnsupportedAttachmentUri_ThrowsException()
    {
        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        Func<Task> act = async () => await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Attachments: new[]
        {
            new Attachment("foo", new Uri("soso://over.there/file.txt"))
        }));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendEmail_SubjectFormatParameters_GetPassedToRenderer()
    {
        Fixture.SmtpServer!.ClearReceivedEmail();
        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        var formatParams = new object[] { 1, "Test" };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, SubjectKey: "SubjectFormat", SubjectFormatParameters: formatParams));

        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.Subject.Should().Be("The formatted subject 1 Test");
    }

    public class SendEmailFixture : TestFixture
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
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            SmtpServer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    [GeneratedRegex("^[a-zA-Z0-9.!-]*$")]
    private static partial Regex GetAsciiCharsRegex();
}