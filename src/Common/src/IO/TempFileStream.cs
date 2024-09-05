// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Common.IO;

/// <summary>
/// A file stream which points to a temporary file. The file automatically gets deleted when the stream is closed.
/// </summary>
public class TempFileStream(int bufferSize = 4096) : FileStream(
    Path.GetTempFileName(),
    FileMode.Create,
    FileAccess.ReadWrite,
    FileShare.Read,
    bufferSize,
    FileOptions.DeleteOnClose);