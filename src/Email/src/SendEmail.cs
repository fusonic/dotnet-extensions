// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Fusonic.Extensions.MediatR;
using MediatR;
using MimeKit;

namespace Fusonic.Extensions.Email;

public static class SendEmail
{
    public class Command : ICommand
    {
#pragma warning disable 8618
        private Command()
#pragma warning restore 8618
        { }

        public Command(
            string recipient,
            string recipientDisplayName,
            CultureInfo culture,
            object viewModel)
            : this(recipient, recipientDisplayName, culture, null, viewModel)
        { }

        public Command(
            string recipient,
            string recipientDisplayName,
            Attachment[] attachments,
            CultureInfo culture,
            object viewModel)
            : this(recipient, recipientDisplayName, null, culture, attachments, null, viewModel)
        { }

        public Command(
            string recipient,
            string recipientDisplayName,
            CultureInfo culture,
            string? subjectKey,
            object viewModel)
            : this(recipient, recipientDisplayName, null, culture, null, subjectKey, viewModel)
        { }

        public Command(
            string recipient,
            string recipientDisplayName,
            string? bccRecipient,
            CultureInfo culture,
            string? subjectKey,
            object viewModel)
            : this(recipient, recipientDisplayName, bccRecipient, culture, null, subjectKey, viewModel)
        { }

        public Command(
            string recipient,
            string recipientDisplayName,
            string? bccRecipient,
            CultureInfo culture,
            Attachment[]? attachments,
            string? subjectKey,
            object viewModel)
        {
            Recipient = recipient;
            RecipientDisplayName = recipientDisplayName;
            BccRecipient = bccRecipient;
            SubjectKey = subjectKey;
            ViewModel = viewModel;
            Culture = culture;
            Attachments = attachments ?? Array.Empty<Attachment>();
        }

        public string Recipient { get; set; }
        public string RecipientDisplayName { get; set; }
        public string? BccRecipient { get; set; }
        public string? SubjectKey { get; set; }
        public object ViewModel { get; set; }
        public CultureInfo Culture { get; set; }
        public Attachment[] Attachments { get; set; }
    }

    [OutOfBand]
    public class Handler : AsyncRequestHandler<Command>, IAsyncDisposable
    {
        private readonly EmailOptions emailOptions;
        private readonly ISmtpClient smtpClient;
        private readonly IEmailRenderingService emailRenderingService;
        private readonly IEmailAttachmentResolver emailAttachmentResolver;

        private readonly List<Stream> openedStreams = new();

        public Handler(EmailOptions emailOptions, ISmtpClient smtpClient, IEmailRenderingService emailRenderingService, IEmailAttachmentResolver emailAttachmentResolver)
        {
            this.emailOptions = emailOptions;
            this.smtpClient = smtpClient;
            this.emailRenderingService = emailRenderingService;
            this.emailAttachmentResolver = emailAttachmentResolver;
        }

        protected override async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var (subject, body) = await emailRenderingService.RenderAsync(request.ViewModel, request.Culture, request.SubjectKey);
            if (emailOptions.SubjectPrefix != null)
                subject = emailOptions.SubjectPrefix + subject;

            var message = new MimeMessage
            {
                Body = await GetMessageBody(body, request.Attachments),
                From = { new MailboxAddress(emailOptions.SenderName, emailOptions.SenderAddress) },
                To = { new MailboxAddress(request.RecipientDisplayName, request.Recipient) },
                Subject = subject
            };

            if (!string.IsNullOrWhiteSpace(request.BccRecipient))
            {
                message.Bcc.Add(new MailboxAddress(request.BccRecipient, request.BccRecipient));
            }

            try
            {
                await smtpClient.SendMailAsync(message);
            }
            finally
            {
                await DisposeStreams();
            }
        }

        private async Task<MimeEntity> GetMessageBody(string htmlBody, Attachment[] attachments)
        {
            var builder = new BodyBuilder { HtmlBody = htmlBody };

            foreach (var attachment in attachments)
            {
                var stream = await emailAttachmentResolver.GetAttachmentAsync(attachment.Source);
                openedStreams.Add(stream);

                var mailAttachment = new MimePart
                {
                    Content = new MimeContent(stream),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = attachment.Name
                };

                builder.Attachments.Add(mailAttachment);
            }

            return builder.ToMessageBody();
        }

        private async Task DisposeStreams()
        {
            foreach (var stream in openedStreams)
            {
                await stream.DisposeAsync();
            }

            openedStreams.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeStreams();
            GC.SuppressFinalize(this);
        }
    }
}
