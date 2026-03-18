// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Fusonic.Extensions.Email.Tests;

public class EmailOptionsTests
{
    [Fact]
    public void IgnoredDomainsCleaned_NormalizesConfiguredDomainsAndIncludesInvalid()
    {
        // Arrange
        var options = new EmailOptions
        {
            IgnoredDomains = [" example.com ", ".Sub.Domain", "EXAMPLE.COM"]
        };

        // Act
        var ignoredDomains = options.IgnoredDomainsCleaned;

        // Assert
        ignoredDomains.Should().BeEquivalentTo([".example.com", ".sub.domain", ".invalid"], o => o.WithStrictOrdering());
    }

    [Fact]
    public void Validate_ValidSmtpOptions_DoesNotThrow()
    {
        // Arrange
        var options = new EmailOptions
        {
            SenderAddress = "sender@fusonic.net",
            SenderName = "Fusonic",
            SmtpServer = "smtp.fusonic.net",
            SmtpPort = 25
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_StoreInDirectorySet_DoesNotRequireSmtpSettings()
    {
        // Arrange
        var options = new EmailOptions
        {
            SenderAddress = "sender@fusonic.net",
            SenderName = "Fusonic",
            StoreInDirectory = "c:\\emails",
            SmtpPort = 0
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_InvalidOptions_ThrowsValidationExceptionWithAllMessages()
    {
        // Arrange
        var options = new EmailOptions
        {
            SmtpPort = 0
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<ValidationException>()
           .Which.Message.Should().ContainAll("SenderAddress is required.", "SmtpServer is required.", "SMTP port is invalid.");
    }
}
