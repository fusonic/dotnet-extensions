// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Fusonic.Extensions.MediatR;
using MediatR;
using MimeKit;

namespace Fusonic.Extensions.Email;

/// <summary>
/// Sends an email in the background.
/// </summary>
/// <param name="Recipient">Email-address of the recipient</param>
/// <param name="RecipientDisplayName">Display name of the recipient</param>
/// <param name="Culture">Culture to render the email in</param>
/// <param name="ViewModel">View model for the email. The model must have an [EmailView]-attribute, providing the path to the email to render (cshtml).</param>
/// <param name="SubjectKey">Subject key to get the subject of the email from the ViewLocalizer. If null, the SubjectKey from the EmailViewAttribute will be used.</param>
/// <param name="BccRecipient">Email-address of the BCC recipient. Optional.</param>
/// <param name="Attachments">Attachments for the email.</param>
/// <param name="SubjectFormatParameters">String formatting parameters for the translated subject. <code>subject = string.Format(subject, SubjectFormatParameters)</code></param>
/// <param name="Headers">Adds the specified Headers to the email and overrides all default headers from the <seealso cref="EmailOptions.DefaultHeaders"/>.</param>
public record SendEmail(
    string Recipient,
    string RecipientDisplayName,
    CultureInfo Culture,
    object ViewModel,
    string? SubjectKey = null,
    string? BccRecipient = null,
    Attachment[]? Attachments = null,
    object[]? SubjectFormatParameters = null,
    IReadOnlyDictionary<string, string>? Headers = null) : ICommand
{
    [OutOfBand]
    public class Handler(EmailOptions emailOptions, ISmtpClient smtpClient, IEmailRenderingService emailRenderingService, IEnumerable<IEmailAttachmentResolver> emailAttachmentResolvers) : AsyncRequestHandler<SendEmail>, IAsyncDisposable
    {
        private readonly List<Stream> openedStreams = [];

        protected override async Task Handle(SendEmail request, CancellationToken cancellationToken)
        {
            var (subject, body) = await emailRenderingService.RenderAsync(request.ViewModel, request.Culture, request.SubjectKey, request.SubjectFormatParameters);
            if (emailOptions.SubjectPrefix != null)
                subject = emailOptions.SubjectPrefix + subject;

            var message = new MimeMessage
            {
                Body = await GetMessageBody(body, request.Attachments, cancellationToken),
                From = { new MailboxAddress(emailOptions.SenderName, emailOptions.SenderAddress) },
                To = { new MailboxAddress(request.RecipientDisplayName, request.Recipient) },
                Subject = subject
            };

            var headers = request.Headers ?? emailOptions.DefaultHeaders;

            if (headers != null)
            {
                foreach (var (field, value) in headers)
                {
                    message.Headers.Add(field, value);
                }
            }

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

        private async Task<MimeEntity> GetMessageBody(string htmlBody, Attachment[]? attachments, CancellationToken cancellationToken)
        {
            var builder = new BodyBuilder { HtmlBody = htmlBody };

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var resolver = GetResolver(attachment.Source);
                    var stream = await resolver.GetAttachmentAsync(attachment.Source, cancellationToken);
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
            }

            return builder.ToMessageBody();
        }

        private IEmailAttachmentResolver GetResolver(Uri uri)
            => emailAttachmentResolvers.FirstOrDefault(r => r.Supports(uri))
            ?? throw new InvalidOperationException("No attachment resolver registered for uri.");

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
