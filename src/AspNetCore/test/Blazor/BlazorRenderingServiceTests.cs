// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using Fusonic.Extensions.AspNetCore.Blazor;

namespace Fusonic.Extensions.AspNetCore.Tests.Blazor;

public partial class BlazorRenderingServiceTests : TestBase
{
    private readonly BlazorRenderingService blazorRenderingService;

    public BlazorRenderingServiceTests(TestFixture fixture) : base(fixture)
    {
        blazorRenderingService = GetInstance<BlazorRenderingService>();
    }

    [Fact]
    public async Task Succeeds()
    {
        // Arrange
        var component = new TestComponent.ComponentModel("Hello World!");

        // Act
        var html = await blazorRenderingService.RenderComponent(component, CultureInfo.CurrentCulture);

        // Assert
        html.Should().Be("<body>Hello World!</body>");
    }

    [Theory]
    [InlineData("de-CH")]
    [InlineData("de-AT")]
    [InlineData("fr-CH")]
    [InlineData("it-CH")]
    [InlineData("en-CH")]
    public async Task Rendering_RespectsCulture(string cultureName)
    {
        // Arrange
        var cultureInfo = new CultureInfo(cultureName);

        var viewModel = new TestCultureComponent.ComponentModel(new(2019, 1, 9), 12345678.89);

        var expectedDate = AsString($"{viewModel.Date:d}");
        var expectedMonth = AsString($"{viewModel.Date:MMMM}");
        var expectedNumber = AsString($"{viewModel.Number:n}");

        // Act
        var body = await blazorRenderingService.RenderComponent(viewModel, cultureInfo);

        // Assert
        var regex = GetHtmlParagraphContentsRegex();
        var matches = regex.Matches(body);

        Match(0).Should().Be(expectedDate);
        Match(1).Should().Be(expectedMonth);
        Match(2).Should().Be(expectedNumber);

        string Match(int idx) => HttpUtility.HtmlDecode(matches[idx].Groups[1].Value);
        string AsString(IFormattable f) => f.ToString(null, cultureInfo);
    }

    [GeneratedRegex("<p>(.*?)<\\/p>")]
    private static partial Regex GetHtmlParagraphContentsRegex();
}
