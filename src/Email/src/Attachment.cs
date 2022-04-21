namespace Fusonic.Extensions.Email;

/// <summary>
/// Attachment when sending emails.
/// </summary>
/// <param name="Name">Filename of the attachment as presented in the email.</param>
/// <param name="Source">The source uri of the attachment. The attachment gets loaded from there using the registered <see cref="IEmailAttachmentResolver"/>.</param>
public record Attachment(string Name, Uri Source);