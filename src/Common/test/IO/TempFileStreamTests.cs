// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Text;
using FluentAssertions;
using Fusonic.Extensions.Common.IO;
using Xunit;

namespace Fusonic.Extensions.Common.Tests.IO;

public class TempFileStreamTests
{
    [Fact]
    public async Task TempFileStream_CreatesFile()
    {
        await using var stream = new TempFileStream();
        File.Exists(stream.Name).Should().BeTrue();
    }

    [Fact]
    public async Task TempFileStream_CanWriteToFile()
    {
        await using var stream = new TempFileStream();
        await stream.WriteAsync("Test"u8.ToArray());
        await stream.FlushAsync();
        
        stream.Seek(0, SeekOrigin.Begin);

        var buffer = new byte[4];
        _ = stream.Read(buffer);

        Encoding.UTF8.GetString(buffer).Should().Be("Test");
    }

    [Fact]
    public async Task TempFileStream_DeletesFileOnDispose()
    {
        string path;
        await using (var stream = new TempFileStream())
        {
            path = stream.Name;
            File.Exists(path).Should().BeTrue();
        }

        File.Exists(path).Should().BeFalse();
    }
}
