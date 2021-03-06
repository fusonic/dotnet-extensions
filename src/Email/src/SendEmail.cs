﻿// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.MediatR;
using MediatR;
using MimeKit;
using MimeKit.Text;

namespace Fusonic.Extensions.Email
{
    public static class SendEmail
    {
        public class Command : ICommand
        {
            #pragma warning disable 8618
            private Command()
            #pragma warning restore 8618
            { }

            public Command(string recipient, string recipientDisplayName, CultureInfo culture, object viewModel) : this(recipient, recipientDisplayName, culture, null, viewModel)
            { }

            public Command(string recipient, string recipientDisplayName, CultureInfo culture, string? subjectKey, object viewModel)
            {
                Recipient = recipient;
                RecipientDisplayName = recipientDisplayName;
                SubjectKey = subjectKey;
                ViewModel = viewModel;
                Culture = culture;
            }

            public string Recipient { get; set; }
            public string RecipientDisplayName { get; set; }
            public string? SubjectKey { get; set; }
            public object ViewModel { get; set; }
            public CultureInfo Culture { get; set; }
        }

        [OutOfBand]
        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly EmailOptions emailOptions;
            private readonly ISmtpClient smtpClient;
            private readonly IEmailRenderingService emailRenderingService;

            public Handler(EmailOptions emailOptions, ISmtpClient smtpClient, IEmailRenderingService emailRenderingService)
            {
                this.emailOptions = emailOptions;
                this.smtpClient = smtpClient;
                this.emailRenderingService = emailRenderingService;
            }

            protected override async Task Handle(Command command, CancellationToken cancellationToken)
            {
                var (subject, body) = await emailRenderingService.RenderAsync(command.ViewModel, command.Culture, command.SubjectKey);
                if (emailOptions.SubjectPrefix != null)
                    subject = emailOptions.SubjectPrefix + subject;

                var message = new MimeMessage
                {
                    Body = new TextPart(TextFormat.Html) { Text = body },
                    From = { new MailboxAddress(emailOptions.SenderName, emailOptions.SenderAddress) },
                    To = { new MailboxAddress(command.RecipientDisplayName, command.Recipient) },
                    Subject = subject
                };
                await smtpClient.SendMailAsync(message);
            }
        }
    }
}