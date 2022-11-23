// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Email;

/// <summary>
/// Used to read the attachments from the specific source that are specified in the Uri when sending an email.
/// </summary>
public interface IEmailAttachmentResolver
{
    Task<Stream> GetAttachmentAsync(Uri uri);
}