using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Fusonic.Extensions.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Xunit;

namespace Fusonic.Extensions.AspNetCore.Tests.Http
{
    public class CultureUtilTests
    {
        private static readonly List<CultureInfo> supportedCultures = new[] { "de-AT", "en-GB", "fr-FR" }.Select(c => new CultureInfo(c)).ToList();

        [Theory]
        [InlineData("", null)]
        [InlineData("Invalid culture...", null)]
        [InlineData("es", null)]
        [InlineData("es-ES", null)]
        [InlineData("de", "de-AT")]
        [InlineData("de-AT", "de-AT")]
        [InlineData("de-DE", "de-AT")]
        [InlineData("en", "en-GB")]
        [InlineData("en-US", "en-GB")]
        public void GetFirstSupportedCulture(string culture, string? expectedCulture)
        {
            var returnedCulture = CultureUtil.GetFirstSupportedCulture(culture, supportedCultures);

            if (expectedCulture == null)
                returnedCulture.Should().BeNull();
            else
                returnedCulture.Should().Be(new CultureInfo(expectedCulture));
        }

        [Fact]
        public void GetFirstSupportedCulture_FallsBackToDefault()
        {
            var returnedCulture = CultureUtil.GetFirstSupportedCulture("Invalid", supportedCultures, new CultureInfo("de-AT"));
            returnedCulture.Should().Be(new CultureInfo("de-AT"));
        }

        [Theory]
        [InlineData("en-US,en;q=0.9,de;q=0.8", "en-GB")]
        [InlineData("en-US;q=0.9,en;q=0.8,de;q=1.0", "de-AT")]
        [InlineData("ru-US,en;q=0.9,de;q=0.8", "en-GB")]
        [InlineData("ru-US,cn;q=0.9,en-GB;q=0.8,de-AT;q=0.7", "en-GB")]
        [InlineData("ru-US,cn;q=0.9,en-GB;q=0.8,de-AT;q=0.85", "de-AT")]
        [InlineData("ru-US,cn;q=0.9,en-GB;q=0.8,fr-AT;q=0.85", "fr-FR")]
        [InlineData("invalid", "de-AT")]
        [InlineData("", "de-AT")]
        [InlineData("de-CH", "de-AT")]
        [InlineData("de-DE", "de-AT")]
        [InlineData("fr-FR", "fr-FR")]
        [InlineData("fr-CH", "fr-FR")]
        public void FromAcceptLanguageHeader(string header, string expectedCulture)
        {
            var acceptLanguage = new RequestHeaders(new HeaderDictionary { ["accept-language"] = header }).AcceptLanguage;

            var returnedCulture = CultureUtil.FromAcceptLanguageHeader(acceptLanguage, supportedCultures, new CultureInfo("de-AT"));

            returnedCulture.Should().Be(new CultureInfo(expectedCulture));
        }
    }
}
