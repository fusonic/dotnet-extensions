using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Fusonic.Extensions.Email.Tests.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using netDumbster.smtp;
using NSubstitute;
using SimpleInjector;
using Xunit;

namespace Fusonic.Extensions.Email.Tests
{
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
            }
        }
    }
}
