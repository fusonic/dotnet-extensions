// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using Fusonic.Extensions.AspNetCore.Razor;

namespace Fusonic.Extensions.AspNetCore.Tests.Razor;

public partial class RazorViewRenderingServiceTests : TestBase
{
    private readonly CultureInfo culture = new("de-AT");
    private readonly RazorViewRenderingService renderingService;

    public RazorViewRenderingServiceTests(TestFixture fixture) : base(fixture)
        => renderingService = GetInstance<RazorViewRenderingService>();

    [Fact]
    public async Task MissingRazorViewModelAttribute_ThrowsArgumentNullException()
    {
        Func<Task> render = async () => await renderingService.RenderAsync(new InvalidViewModel(), culture);

        await render.Should()
                    .ThrowAsync<ArgumentNullException>()
                    .WithMessage($"Value cannot be null. (Parameter 'The Model {nameof(InvalidViewModel)} is missing a {nameof(RazorViewModelAttribute)}.')");
    }

    [Fact]
    public async Task MissingView_ThrowsFileNotFoundException()
    {
        Func<Task> render = async () => await renderingService.RenderAsync(new MissingViewModel(), culture);

        await render.Should()
                    .ThrowAsync<FileNotFoundException>()
                    .WithMessage("The View Tests/MissingViewFile could not be found. Searched locations: /Views/Tests/MissingViewFile.cshtml, /Views/Shared/Tests/MissingViewFile.cshtml, /Pages/Shared/Tests/MissingViewFile.cshtml");
    }

    [Theory]
    [InlineData("de-CH")]
    [InlineData("de-AT")]
    [InlineData("fr-CH")]
    [InlineData("it-CH")]
    [InlineData("en-CH")]
    public async Task Rendering_RespectsCulture(string cultureName)
    {
        var cultureInfo = new CultureInfo(cultureName);

        var emailRenderingService = GetInstance<RazorViewRenderingService>();
        var viewModel = new TestViewModel();

        var expectedDate = AsString($"{viewModel.Date:d}");
        var expectedMonth = AsString($"{viewModel.Date:MMMM}");
        var expectedNumber = AsString($"{viewModel.Number:n}");

        var body = await emailRenderingService.RenderAsync(viewModel, cultureInfo);

        var regex = GetHtmlParagraphContentsRegex();
        var matches = regex.Matches(body);

        Match(0).Should().Be(expectedDate);
        Match(1).Should().Be(expectedMonth);
        Match(2).Should().Be(expectedNumber);

        string Match(int idx) => HttpUtility.HtmlDecode(matches[idx].Groups[1].Value);
        string AsString(IFormattable f) => f.ToString(null, cultureInfo);
    }

    [RazorViewModel("Tests/RazorRenderingTest")]
    public class TestViewModel
    {
        public DateTime Date { get; set; } = new(2019, 1, 9);
        public double Number { get; set; } = 1234567.89;
    }

    [RazorViewModel("Tests/MissingViewFile")]
    public class MissingViewModel
    { }

    public class InvalidViewModel
    { }

    [GeneratedRegex("<p>(.*?)<\\/p>")]
    private static partial Regex GetHtmlParagraphContentsRegex();
}
