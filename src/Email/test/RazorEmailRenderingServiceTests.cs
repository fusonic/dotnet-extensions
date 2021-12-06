// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using FluentAssertions;
using Fusonic.Extensions.Email.Tests.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace Fusonic.Extensions.Email.Tests;

public class RazorEmailRenderingServiceTests : TestBase
{
    private readonly CultureInfo culture = new("de-AT");
    private readonly RazorEmailRenderingService emailRenderingService;

    public RazorEmailRenderingServiceTests(TestFixture fixture) : base(fixture)
        => emailRenderingService = GetInstance<RazorEmailRenderingService>();

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
        var (subject, body) = await emailRenderingService.RenderAsync(new CultureTestEmailViewModel(), culture, subjectKey: null);
        subject.Should().Be("Subject");
        body.Should()
            .Contain("body style=\"color: red\"");
    }

    [Fact]
    public async Task ReturnsSubjectFromResource_OrCustomSubjectAsFallback()
    {
        const string subjectKey = "CustomSubjectKey";
        var (subject, _) = await emailRenderingService.RenderAsync(new CultureTestEmailViewModel(), culture, subjectKey: subjectKey);
        subject.Should().Be(subjectKey);

        var localizer = Container.GetInstance<Func<IViewLocalizer>>()();
        localizer.GetString(subjectKey).Returns(new LocalizedString(subjectKey, "My fancy subject localized"));

        (subject, _) = await emailRenderingService.RenderAsync(new CultureTestEmailViewModel(), culture, subjectKey: subjectKey);
        subject.Should().Be("My fancy subject localized");
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

        var emailRenderingService = GetInstance<RazorEmailRenderingService>();
        var viewModel = new CultureTestEmailViewModel();

        var expectedDate = ToString($"{viewModel.Date:d}");
        var expectedMonth = ToString($"{viewModel.Date:MMMM}");
        var expectedNumber = ToString($"{viewModel.Number:n}");

        var (_, body) = await emailRenderingService.RenderAsync(viewModel, cultureInfo, subjectKey: null);

        var regex = new Regex("<p>(.*?)<\\/p>");
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
}
