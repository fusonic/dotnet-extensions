// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using AwesomeAssertions;
using Fusonic.Extensions.Email.Tests.Models;
using MimeKit;
using NSubstitute;
using NSubstitute.ClearExtensions;
using SimpleInjector;
using Xunit;

namespace Fusonic.Extensions.Email.Tests;

public class SendEmailWithMockedClientTests : TestBase<SendEmailWithMockedClientTests.SendEmailFixture>
{
    private readonly ISmtpClient smtpClient;

    public SendEmailWithMockedClientTests(SendEmailFixture fixture) : base(fixture)
    {
        smtpClient = GetInstance<ISmtpClient>();
        smtpClient.ClearSubstitute();
    }

    [Fact]
    public async Task SendEmail_StandardHeadersOverwritten()
    {
        await smtpClient.SendMailAsync(Arg.Do<MimeMessage>(ValidateEmail));

        var model = new SendEmailTestEmailViewModel { SomeField = "Some field." };
        await SendAsync(new SendEmail("recipient@fusonic.net", "The Recipient", new CultureInfo("de-AT"), model, Headers: new Dictionary<string, string> { ["Message-Id"] = "value" }));

        await smtpClient.Received(1).SendMailAsync(Arg.Any<MimeMessage>());

        static void ValidateEmail(MimeMessage email)
        {
            email.Headers.Should().ContainSingle(s => s.Field.Equals("Message-Id", StringComparison.OrdinalIgnoreCase));
            email.Headers["Message-Id"].Should().Be("value");
        }
    }

    public class SendEmailFixture : TestFixture
    {
        protected override void RegisterDependencies(Container container)
        {
            container.RegisterSingleton(() => Substitute.For<ISmtpClient>());
        }
    }
}