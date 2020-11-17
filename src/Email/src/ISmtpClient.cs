// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Threading.Tasks;
using MimeKit;

namespace Fusonic.Extensions.Email
{
    public interface ISmtpClient
    {
        Task SendMailAsync(MimeMessage message);
    }
}