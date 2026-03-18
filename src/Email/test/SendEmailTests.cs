// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Fusonic.Extensions.Email.Tests.Models;
using MailKit.Net.Smtp;
using netDumbster.smtp;
using SimpleInjector;

namespace Fusonic.Extensions.Email.Tests;

public partial class SendEmailTests(SendEmailTests.SendEmailFixture fixture) : TestBase<SendEmailTests.SendEmailFixture>(fixture)
{
    [Fact]
    public async Task SendEmail_Razor()
    {
        // Arrange
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        // Assert
        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.ToAddresses
             .Should()
             .ContainSingle()
             .And.Contain(a => a.Address == "recipient@fusonic.net");

        email.FromAddress.Address.Should().Be("test@fusonic.net");
        email.Headers.AllKeys.Should().Contain("Subject");
        email.Headers["Subject"].Should().Be("The subject");

        email.MessageParts.Should().ContainSingle();
        email.MessageParts[0]
             .BodyData.Should()
             .Be(
                  "<html><head><title>Render Test</title></head>" + Environment.NewLine
                                                                  + "<body style=\"color: red\"><p>Some field.</p>" + Environment.NewLine
                                                                  + "</body></html>");
    }

    [Fact]
    public async Task SendEmail_Blazor()
    {
        // Arrange
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailComponentModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        // Assert
        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.ToAddresses
             .Should()
             .ContainSingle()
             .And.Contain(a => a.Address == "recipient@fusonic.net");

        email.FromAddress.Address.Should().Be("test@fusonic.net");
        email.Headers.AllKeys.Should().Contain("Subject");
        email.Headers["Subject"].Should().Be("The subject");

        email.MessageParts.Should().ContainSingle();
        email.MessageParts[0]
             .BodyData.Should()
             .Be(
                  "<html><head><title>Render Test</title></head>" + Environment.NewLine
                                                                  + "<body style=\"color: red\"><p>Some field.</p></body></html>");
    }

    [Fact]
    public async Task SendEmail_BccAdded()
    {
        // Arrange
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, BccRecipient: "bcc@fusonic.net"));

        // Assert
        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.ToAddresses.Select(a => a.Address).Should().BeEquivalentTo(["recipient@fusonic.net"]);
        email.BccAddresses.Select(a => a.Address).Should().BeEquivalentTo(["bcc@fusonic.net"]);

        email.FromAddress.Address.Should().Be("test@fusonic.net");
        email.Headers.AllKeys.Should().Contain("Subject");
        email.Headers["Subject"].Should().Be("The subject");

        email.MessageParts.Should().ContainSingle();
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
        // Arrange
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

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Attachments: attachments));

        // Assert
        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.MessageParts.Should().HaveCount(2);

        if (!GetAsciiCharsRegex().IsMatch(attachmentName))
        {
            var base64EncodedName = Convert.ToBase64String(Encoding.UTF8.GetBytes(attachmentName));
            email.MessageParts[1].HeaderData.Should().Contain($"application/octet-stream; name=\"=?utf-8?B?{base64EncodedName}?=\"");
        }
        else
        {
            email.MessageParts[1].HeaderData.Should().Contain(attachmentName);
        }

        var expectedAttachmentContent = await File.ReadAllTextAsync(attachmentPath, TestContext.Current.CancellationToken);
        email.MessageParts[1].BodyData.Should().Be(expectedAttachmentContent);
    }

    [Fact]
    public async Task SendEmail_HeadersAdded()
    {
        // Arrange
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Headers: new Dictionary<string, string> { ["my-header"] = "value" }));

        // Assert
        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();

        email.Headers.AllKeys.Should().Contain("my-header");
        email.Headers["my-header"].Should().Be("value");
    }

    [Fact]
    public async Task SendEmail_DefaultHeadersAdded()
    {
        // Arrange
        Fixture.SmtpServer!.ClearReceivedEmail();

        var options = GetInstance<EmailOptions>();

        options.DefaultHeaders = new Dictionary<string, string> { ["my-header"] = "value" };

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        // Assert
        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();

        email.Headers.AllKeys.Should().Contain("my-header");
        email.Headers["my-header"].Should().Be("value");
    }

    [Fact]
    public async Task SendEmail_DefaultHeadersOverriddenIfSetTwice()
    {
        // Arrange
        Fixture.SmtpServer!.ClearReceivedEmail();

        var options = GetInstance<EmailOptions>();

        options.DefaultHeaders = new Dictionary<string, string> { ["replaced"] = "value", ["default"] = "value" };

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Headers: new Dictionary<string, string> { ["replaced"] = "new-value" }));

        // Assert
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
        // Arrange
        Fixture.SmtpServer!.ClearReceivedEmail();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, ReplyTo: "reply@mail.com"));

        // Assert
        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();

        email.Headers.AllKeys.Should().Contain("Reply-To");
        email.Headers["Reply-To"].Should().Be("reply@mail.com");
    }

    [Fact]
    public async Task SendEmail_InvalidBccEmailAddress_ThrowsException()
    {
        // Arrange
        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        var act = async () => await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, BccRecipient: "invalidEmailAddress"));

        // Assert
        await act.Should().ThrowAsync<SmtpCommandException>();
    }

    [Fact]
    public async Task SendEmail_UnsupportedAttachmentUri_ThrowsException()
    {
        // Arrange
        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        var act = async () => await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Attachments: [
            new Attachment("foo", new Uri("soso://over.there/file.txt"))
        ]));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendEmail_SubjectFormatParameters_GetPassedToRenderer()
    {
        // Arrange
        Fixture.SmtpServer!.ClearReceivedEmail();
        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        var formatParams = new object[] { 1, "Test" };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, SubjectKey: "SubjectFormat", SubjectFormatParameters: formatParams));

        // Assert
        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(1);
        var email = Fixture.SmtpServer.ReceivedEmail.Single();
        email.Subject.Should().Be("The formatted subject 1 Test");
    }

    [Theory]
    [InlineData("user@test.invalid")]
    [InlineData("user@sub.domain.invalid")]
    [InlineData("user@invalid")]
    [InlineData("invalid")]
    [InlineData("user@SUB.DOMAIN.INVAlid")]
    [InlineData("user@DoMaIn.InValid")]
    public async Task SendEmail_IgnoredDomain_DoesNotSendEmail(string recipient)
    {
        // Arrange
        Fixture.SmtpServer!.ClearReceivedEmail();
        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail(recipient, recipient, new CultureInfo("de-AT"), model));

        // Assert
        Fixture.SmtpServer.ReceivedEmailCount.Should().Be(0);
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

        public override async ValueTask DisposeAsync()
        {
            SmtpServer?.Dispose();
            await base.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }

    [GeneratedRegex("^[a-zA-Z0-9.!-]*$")]
    private static partial Regex GetAsciiCharsRegex();
}