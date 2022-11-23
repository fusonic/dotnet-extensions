// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Email;

/// <summary>
/// Reads attachments from the local file system.
/// </summary>
public class FileAttachmentResolver : IEmailAttachmentResolver
{
    public Task<Stream> GetAttachmentAsync(Uri uri)
    {
        if (uri.Scheme != "file")
            throw new ArgumentException("Unsupported scheme.", nameof(uri));

        var stream = File.OpenRead(uri.LocalPath);
        return Task.FromResult((Stream)stream);
    }
}