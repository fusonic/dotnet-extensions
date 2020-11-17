// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using FluentAssertions;
using Fusonic.Extensions.Common.IO;
using Xunit;

namespace Fusonic.Extensions.Common.Tests.IO
{
    public class PathUtilTests
    {
        [Theory]
        [InlineData("some/path.pdf", "somepath.pdf")]
        [InlineData("<>*/", "")]
        [InlineData("", "")]
        [InlineData("There may< be som*e ty?pos", "There may be some typos")]
        [InlineData("*.pdf", ".pdf")]
        [InlineData("/*.pdf", ".pdf")]
        public void RemoveInvalidFileChars(string input, string expected) 
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
}