// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using FluentAssertions;
using FluentAssertions.Execution;
using Fusonic.Extensions.AspNetCore.Http.Middlewares;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Fusonic.Extensions.AspNetCore.Tests.Http;

public class IgnorePathsMiddlewareTests
{
    [Theory]
    [InlineData("/", false)]
    [InlineData("/login", false)]
    [InlineData("/apiOverview", false)]
    [InlineData("/api/overview", true)]
    [InlineData("/api", true)]
    [InlineData("/ignore", true)]
    [InlineData("/user/ignore", false)]
    public async Task IgnorePaths(string path, bool shouldIgnore)
    {
        var httpContext = new DefaultHttpContext();
        var nextCalled = false;

        var expectedHttpStatusCode = shouldIgnore ? 404 : 200;
        var expectNextCall = !shouldIgnore;

        var middleware = new IgnorePathsMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            new List<PathString> { "/ignore", "/api" });

        httpContext.Request.Path = path;
        await middleware.Invoke(httpContext);

        using var _ = new AssertionScope();
        nextCalled.Should().Be(expectNextCall);
        httpContext.Response.StatusCode.Should().Be(expectedHttpStatusCode);
    }
}
