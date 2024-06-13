// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using FluentAssertions;
using Fusonic.Extensions.Email.Tests.Models;
using Xunit;

namespace Fusonic.Extensions.Email.Tests;

public partial class BlazorEmailRenderingServiceTests : TestBase
{
    private readonly CultureInfo culture = new("de-AT");
    private readonly BlazorEmailRenderingService emailRenderingService;

    public BlazorEmailRenderingServiceTests(TestFixture fixture) : base(fixture)
        => emailRenderingService = GetInstance<IEnumerable<IEmailRenderingService>>().OfType<BlazorEmailRenderingService>().Single();

    [Fact]
    public async Task MissingIComponentModelImplementation_ThrowsArgumentException()
    {
        Func<Task> render = async () => await emailRenderingService.RenderAsync(new InvalidComponentModel(), culture, subjectKey: null);

        await render.Should()
                    .ThrowAsync<ArgumentException>()
                    .WithMessage($"The type {typeof(InvalidComponentModel).Name} does not implement {nameof(IComponentModel)}. (Parameter 'model')");
    }

    [Fact]
    public async Task ValidComponent_ReturnRenderedResults()
    {
        var (subject, body) = await emailRenderingService.RenderAsync(new RazorRenderingTestEmailComponentModel(), culture, subjectKey: null);
        subject.Should().Be("This is the default subject");
        body.Should()
            .Contain("body style=\"color: red\"");
    }

    [Fact]
    public async Task Subject_NotSet_FallsBackToDefaultKey_Subject()
    {
        var (subject, _) = await emailRenderingService.RenderAsync(new RazorRenderingTestEmailComponentModel(), culture, subjectKey: null);
        subject.Should().Be("This is the default subject");
    }

    [Fact]
    public async Task Subject_KeyExistsInResource_ReturnsLocalizedSubject()
    {
        var (subject, _) = await emailRenderingService.RenderAsync(new RazorRenderingTestEmailComponentModel(), culture, subjectKey: "FancySubject");
        subject.Should().Be("This is a fancy subject");
    }

    [Fact]
    public async Task Subject_KeyDoesNotExistInResource_ReturnsKey()
    {
        var (subject, _) = await emailRenderingService.RenderAsync(new RazorRenderingTestEmailComponentModel(), culture, subjectKey: "Some custom title");
        subject.Should().Be("Some custom title");
    }

    [Fact]
    public async Task Subject_ValueHasFormatParameters_Formats()
    {
        var (subject, _) = await emailRenderingService.RenderAsync(new RazorRenderingTestEmailComponentModel(), culture, subjectKey: "FormattedSubject", subjectFormatParameters: ["Oh hi", new DateTime(2020, 2, 3), Guid.Empty]);
        subject.Should().Be("This is a formatted subject Oh hi 2020-02-03 00000000000000000000000000000000");
    }

    [Theory]
    [InlineData("de-CH", "de")]
    [InlineData("fr-CH", "fr")]
    [InlineData("it-CH", "it")]
    [InlineData("en-CH", "en")]
    public async Task Subject_RespectsCulture(string culture, string expectedSubject)
    {
        var cultureInfo = new CultureInfo(culture);

        var componentModel = new RazorSubjectTestEmailComponentModel();

        var (subject, _) = await emailRenderingService.RenderAsync(componentModel, cultureInfo, subjectKey: null);

        subject.Should().Be(expectedSubject);
    }

    [Theory]
    [InlineData("de-CH")]
    [InlineData("de-AT")]
    [InlineData("fr-CH")]
    [InlineData("it-CH")]
    [InlineData("en-CH")]
    public async Task Rendering_RespectsCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);

        var componentModel = new RazorRenderingTestEmailComponentModel();

        var expectedDate = ToString($"{componentModel.Date:d}");
        var expectedMonth = ToString($"{componentModel.Date:MMMM}");
        var expectedNumber = ToString($"{componentModel.Number:n}");

        var (_, body) = await emailRenderingService.RenderAsync(componentModel, cultureInfo, subjectKey: null);

        var regex = GetHtmlParagraphContentsRegex();
        var matches = regex.Matches(body);

        Match(0).Should().Be(expectedDate);
        Match(1).Should().Be(expectedMonth);
        Match(2).Should().Be(expectedNumber);

        string Match(int idx) => HttpUtility.HtmlDecode(matches[idx].Groups[1].Value);
        string ToString(IFormattable f) => f.ToString(null, cultureInfo);
    }

    public class InvalidComponentModel
    { }

    [GeneratedRegex("<p>(.*?)<\\/p>")]
    private static partial Regex GetHtmlParagraphContentsRegex();
}
