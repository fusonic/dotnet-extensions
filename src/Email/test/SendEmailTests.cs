// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Fusonic.Extensions.Email.Tests.Models;
using MailKit.Net.Smtp;
using MimeKit;
using SimpleInjector;

namespace Fusonic.Extensions.Email.Tests;

[Collection("Mailpit")]
public class SendEmailTests(SendEmailTests.SendEmailFixture fixture) : TestBase<SendEmailTests.SendEmailFixture>(fixture)
{
    [Fact]
    public async Task SendEmail_Razor()
    {
        // Arrange
        await Mailpit.DeleteEmailsAsync();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        // Assert
        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        var message = email.Message;
        message.To.Mailboxes.Select(a => a.Address).Should().BeEquivalentTo(["recipient@fusonic.net"]);


        message.From.Mailboxes.Single().Address.Should().Be("test@fusonic.net");
        message.Subject.Should().Be("The subject");

        message.BodyParts.Should().ContainSingle().Which.Should().BeOfType<TextPart>().Which.Text.TrimEnd().ReplaceLineEndings().Should()
             .Be(
                  "<html><head><title>Render Test</title></head>" + Environment.NewLine
                                                                  + "<body style=\"color: red\"><p>Some field.</p>" + Environment.NewLine
                                                                  + "</body></html>");
    }

    [Fact]
    public async Task SendEmail_Blazor()
    {
        // Arrange
        await Mailpit.DeleteEmailsAsync();

        var model = new SendEmailTestEmailComponentModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        // Assert
        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        var message = email.Message;
        message.To.Mailboxes.Select(a => a.Address).Should().BeEquivalentTo(["recipient@fusonic.net"]);


        message.From.Mailboxes.Single().Address.Should().Be("test@fusonic.net");
        message.Subject.Should().Be("The subject");

        message.BodyParts.Should().ContainSingle().Which.Should().BeOfType<TextPart>().Which.Text.TrimEnd().ReplaceLineEndings().Should()
             .Be(
                  "<html><head><title>Render Test</title></head>" + Environment.NewLine
                                                                  + "<body style=\"color: red\"><p>Some field.</p></body></html>");
    }

    [Fact]
    public async Task SendEmail_BccAdded()
    {
        // Arrange
        await Mailpit.DeleteEmailsAsync();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, BccRecipient: "bcc@fusonic.net"));

        // Assert
        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        var message = email.Message;
        message.To.Mailboxes.Select(a => a.Address).Should().BeEquivalentTo(["recipient@fusonic.net"]);
        email.BccAddresses.Should().BeEquivalentTo(["bcc@fusonic.net"]);

        message.From.Mailboxes.Single().Address.Should().Be("test@fusonic.net");
        message.Subject.Should().Be("The subject");

        message.BodyParts.Should().ContainSingle().Which.Should().BeOfType<TextPart>().Which.Text.TrimEnd().ReplaceLineEndings().Should()
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
        await Mailpit.DeleteEmailsAsync();

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
        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        var message = email.Message;
        message.BodyParts.Should().HaveCount(2);
        var attachment = message.Attachments.Should().ContainSingle().Which.Should().BeOfType<MimePart>().Subject;
        attachment.FileName.Should().Be(attachmentName);

        using var content = new MemoryStream();
        await attachment.Content!.DecodeToAsync(content, TestContext.Current.CancellationToken);
        content.ToArray().Should().Equal(await File.ReadAllBytesAsync(attachmentPath, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SendEmail_HeadersAdded()
    {
        // Arrange
        await Mailpit.DeleteEmailsAsync();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Headers: new Dictionary<string, string> { ["my-header"] = "value" }));

        // Assert
        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        var message = email.Message;

        message.Headers["my-header"].Should().Be("value");
    }

    [Fact]
    public async Task SendEmail_DefaultHeadersAdded()
    {
        // Arrange
        await Mailpit.DeleteEmailsAsync();

        var options = GetInstance<EmailOptions>();

        options.DefaultHeaders = new Dictionary<string, string> { ["my-header"] = "value" };

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model));

        // Assert
        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        var message = email.Message;

        message.Headers["my-header"].Should().Be("value");
    }

    [Fact]
    public async Task SendEmail_DefaultHeadersOverriddenIfSetTwice()
    {
        // Arrange
        await Mailpit.DeleteEmailsAsync();

        var options = GetInstance<EmailOptions>();

        options.DefaultHeaders = new Dictionary<string, string> { ["replaced"] = "value", ["default"] = "value" };

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Headers: new Dictionary<string, string> { ["replaced"] = "new-value" }));

        // Assert
        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        var message = email.Message;

        message.Headers["replaced"].Should().Be("new-value");
        message.Headers["default"].Should().Be("value");
    }

    [Fact]
    public async Task SendEmail_ReplyToAdded()
    {
        // Arrange
        await Mailpit.DeleteEmailsAsync();

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, ReplyTo: "reply@mail.com"));

        // Assert
        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        var message = email.Message;

        message.ReplyTo.ToString().Should().Be("reply@mail.com");
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
        await Mailpit.DeleteEmailsAsync();
        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        var formatParams = new object[] { 1, "Test" };

        // Act
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, SubjectKey: "SubjectFormat", SubjectFormatParameters: formatParams));

        // Assert
        var email = (await Mailpit.GetEmailsAsync()).Should().ContainSingle().Subject;
        var message = email.Message;
        message.Subject.Should().Be("The formatted subject 1 Test");
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
        await Mailpit.DeleteEmailsAsync();
        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };

        // Act
        await SendAsync(new SendEmail(recipient, recipient, new CultureInfo("de-AT"), model));

        // Assert
        (await Mailpit.GetEmailsAsync()).Should().BeEmpty();
    }

    public class SendEmailFixture : TestFixture
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
            });
        }
    }
}