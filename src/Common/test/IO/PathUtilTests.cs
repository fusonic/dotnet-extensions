// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using FluentAssertions;
using Fusonic.Extensions.Common.IO;
using Xunit;

namespace Fusonic.Extensions.Common.Tests.IO;

public class PathUtilTests
{
    [Theory]
    [InlineData("some/path.pdf", true)]
    [InlineData("<>*/", true)]
    [InlineData("", false)]
    [InlineData("There may< be som*e ty?pos", true)]
    [InlineData("hi.pdf", false)]
    public void HasInvalidFilenameChars(string input, bool expected)
        => PathUtil.HasInvalidFilenameChar(input).Should().Be(expected);

    [Theory]
    [InlineData("some/path.pdf", false)]
    [InlineData("some/ot?her/pat*h.pdf", true)]
    [InlineData("", false)]
    [InlineData(".pdf", false)]
    [InlineData("/.pdf", false)]
    [InlineData("/*.pdf", true)]
    public void HasInvalidPathChars(string input, bool expected)
        => PathUtil.HasInvalidPathChar(input).Should().Be(expected);

    [Theory]
    [InlineData("some/path.pdf", "somepath.pdf")]
    [InlineData("<>*/", "")]
    [InlineData("", "")]
    [InlineData("There may< be som*e ty?pos", "There may be some typos")]
    [InlineData("*.pdf", ".pdf")]
    [InlineData("/*.pdf", ".pdf")]
    public void RemoveInvalidFilenameChars(string input, string expected)
        => PathUtil.RemoveInvalidFilenameChars(input).Should().Be(expected);

    [Theory]
    [InlineData("some/path.pdf", "some/path.pdf")]
    [InlineData("some/ot?her/pat*h.pdf", "some/other/path.pdf")]
    [InlineData("<>*/", "/")]
    [InlineData("", "")]
    [InlineData("There may< be som*e ty?pos", "There may be some typos")]
    [InlineData("*.pdf", ".pdf")]
    [InlineData("/*.pdf", "/.pdf")]
    public void RemoveInvalidPathChars(string input, string expected)
        => PathUtil.RemoveInvalidPathChars(input).Should().Be(expected);
}
