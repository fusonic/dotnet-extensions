// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Email;

/// <summary>
/// Used to read the attachments from the specific source that are specified in the Uri when sending an email.
/// </summary>
public interface IEmailAttachmentResolver
{
    /// <summary>
    /// Gets if the resolver supports the given Uri.
    /// </summary>
    bool Supports(Uri uri);

    /// <summary>
    /// Gets the attachment for the given Uri. Only gets called if the Uri is supported. Caller is responsible of disposing the returned stream.
    /// </summary>
    Task<Stream> GetAttachmentAsync(Uri uri, CancellationToken cancellationToken);
}