// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Common.IO;
using MailKit.Security;
using MimeKit;

namespace Fusonic.Extensions.Email;

public class SmtpClient(EmailOptions options) : ISmtpClient
{
    public async Task SendMailAsync(MimeMessage message)
    {
        if (!string.IsNullOrWhiteSpace(options.StoreInDirectory))
        {
            string path = Path.Combine(options.StoreInDirectory, $"{DateTime.Now:yyyyMMdd_HHmmss}_{PathUtil.RemoveInvalidFilenameChars(message.Subject)}_{Guid.NewGuid()}.eml");
            await message.WriteToAsync(path);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(options.SmtpServer))
                throw new InvalidOperationException("Neither an SMTP server nor a directory to store the mails in is configured.");

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(options.SmtpServer, options.SmtpPort, options.EnableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None);
            if (!string.IsNullOrWhiteSpace(options.SmtpUsername))
            {
                await client.AuthenticateAsync(options.SmtpUsername, options.SmtpPassword);
            }
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
