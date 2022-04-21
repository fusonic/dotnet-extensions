namespace Fusonic.Extensions.Email;

/// <summary>
/// Used to read the attachments from the specific source that are specified in the Uri when sending an email.
/// </summary>
public interface IEmailAttachmentResolver
{
    Task<Stream> GetAttachmentAsync(Uri uri);
}