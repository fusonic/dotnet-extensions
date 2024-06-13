// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using FluentAssertions;
using Fusonic.Extensions.Email.Tests.Models;
using Xunit;

namespace Fusonic.Extensions.Email.Tests;

public partial class RazorEmailRenderingServiceTests : TestBase
{
    private readonly CultureInfo culture = new("de-AT");
    private readonly RazorEmailRenderingService emailRenderingService;

    public RazorEmailRenderingServiceTests(TestFixture fixture) : base(fixture)
        => emailRenderingService = GetInstance<IEnumerable<IEmailRenderingService>>().OfType<RazorEmailRenderingService>().Single();

    [Fact]
    public async Task MissingEmailViewAttribute_ThrowsArgumentNullException()
    {
        Func<Task> render = async () => await emailRenderingService.RenderAsync(new InvalidViewModel(), culture, subjectKey: null);

        await render.Should()
                    .ThrowAsync<ArgumentNullException>()
                    .WithMessage($"Value cannot be null. (Parameter 'The Model {nameof(InvalidViewModel)} is missing an EmailViewAttribute.')");
    }

    [Fact]
    public async Task MissingView_ThrowsFileNotFoundException()
    {
        Func<Task> render = async () => await emailRenderingService.RenderAsync(new MissingViewModel(), culture, subjectKey: null);

        await render.Should()
                    .ThrowAsync<FileNotFoundException>()
                    .WithMessage("The View Emails/MissingViewFile could not be found. Searched locations: /Views/Emails/MissingViewFile.cshtml, /Views/Shared/Emails/MissingViewFile.cshtml, /Pages/Shared/Emails/MissingViewFile.cshtml");
    }

    [Fact]
    public async Task ValidView_ReturnRenderedResults()
    {
        var (subject, body) = await emailRenderingService.RenderAsync(new RazorRenderingTestEmailViewModel(), culture, subjectKey: null);
        subject.Should().Be("This is the default subject");
        body.Should()
            .Contain("body style=\"color: red\"");
    }

    [Fact]
    public async Task Subject_NotSet_FallsBackToDefaultKey_Subject()
    {
        var (subject, _) = await emailRenderingService.RenderAsync(new RazorRenderingTestEmailViewModel(), culture, subjectKey: null);
        subject.Should().Be("This is the default subject");
    }

    [Fact]
    public async Task Subject_KeyExistsInResource_ReturnsLocalizedSubject()
    {
        var (subject, _) = await emailRenderingService.RenderAsync(new RazorRenderingTestEmailViewModel(), culture, subjectKey: "FancySubject");
        subject.Should().Be("This is a fancy subject");
    }

    [Fact]
    public async Task Subject_KeyDoesNotExistInResource_ReturnsKey()
    {
        var (subject, _) = await emailRenderingService.RenderAsync(new RazorRenderingTestEmailViewModel(), culture, subjectKey: "Some custom title");
        subject.Should().Be("Some custom title");
    }

    [Fact]
    public async Task Subject_ValueHasFormatParameters_Formats()
    {
        var (subject, _) = await emailRenderingService.RenderAsync(new RazorRenderingTestEmailViewModel(), culture, subjectKey: "FormattedSubject", subjectFormatParameters: ["Oh hi", new DateTime(2020, 2, 3), Guid.Empty]);
        subject.Should().Be("This is a formatted subject Oh hi 2020-02-03 00000000000000000000000000000000");
    }

    [Theory]
    [InlineData("de-CH")]
    [InlineData("de-AT")]
    [InlineData("fr-CH")]
    [InlineData("it-CH")]
    [InlineData("en-CH")]
    public async Task RespectsCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);

        var viewModel = new RazorRenderingTestEmailViewModel();

        var expectedDate = ToString($"{viewModel.Date:d}");
        var expectedMonth = ToString($"{viewModel.Date:MMMM}");
        var expectedNumber = ToString($"{viewModel.Number:n}");

        var (_, body) = await emailRenderingService.RenderAsync(viewModel, cultureInfo, subjectKey: null);

        var regex = GetHtmlParagraphContentsRegex();
        var matches = regex.Matches(body);

        Match(0).Should().Be(expectedDate);
        Match(1).Should().Be(expectedMonth);
        Match(2).Should().Be(expectedNumber);

        string Match(int idx) => HttpUtility.HtmlDecode(matches[idx].Groups[1].Value);
        string ToString(IFormattable f) => f.ToString(null, cultureInfo);
    }

    [EmailView("Emails/MissingViewFile")]
    public class MissingViewModel
    { }

    public class InvalidViewModel
    { }

    [GeneratedRegex("<p>(.*?)<\\/p>")]
    private static partial Regex GetHtmlParagraphContentsRegex();
}
