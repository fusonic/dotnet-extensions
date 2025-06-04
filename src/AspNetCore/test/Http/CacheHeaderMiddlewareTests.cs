// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.AspNetCore.Http.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Fusonic.Extensions.AspNetCore.Tests.Http;

public class CacheHeaderMiddlewareTests
{
    [Fact]
    public async Task SetDefaultHeaderIfNoSpecificRouteIsSet()
    {
        var defaultHeader = new CacheControlHeaderValue { NoCache = true };
        var httpContext = new DefaultHttpContext();
        var nextCalled = false;

        var middleware = new CacheHeaderMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }, new CacheHeaderOptions
        {
            DefaultHeader = defaultHeader
        });

        await middleware.Invoke(httpContext);
        httpContext.Response.GetTypedHeaders().CacheControl.Should().Be(defaultHeader);
        nextCalled.Should().BeTrue();

        // Does not override existing header
        nextCalled = false;
        
        await middleware.Invoke(httpContext);
        httpContext.Response.GetTypedHeaders().CacheControl.Should().Be(defaultHeader);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task SetRouteHeaderIfRouteMatches()
    {
        var defaultHeader = new CacheControlHeaderValue { NoCache = true };
        var routeHeader = new CacheControlHeaderValue { NoCache = false, MaxAge = TimeSpan.FromDays(10) };
        var httpContext = new DefaultHttpContext() { Request = { Path = "/SomeOtherRoute" } };
        var nextCalled = false;
        var middleware = new CacheHeaderMiddleware(new RequestDelegate(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }), new CacheHeaderOptions()
        {
            DefaultHeader = defaultHeader,
            Routes = { { "/MyFancyRoute", routeHeader } }
        });

        await middleware.Invoke(httpContext);
        httpContext.Response.GetTypedHeaders().CacheControl.Should().Be(defaultHeader);
        nextCalled.Should().BeTrue();

        nextCalled = false;
        httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/MyFancyRoute/xyz/random";
        await middleware.Invoke(httpContext);
        httpContext.Response.GetTypedHeaders().CacheControl.Should().Be(routeHeader);
        nextCalled.Should().BeTrue();

        // Check route matching ignores case (Ordinal ignore case)
        httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/myFancyRoute/xyz/random";
        await middleware.Invoke(httpContext);
        httpContext.Response.GetTypedHeaders().CacheControl.Should().Be(routeHeader);
    }

    [Fact]
    public void ConfigureValueForRoutes()
    {
        var options = new CacheHeaderOptions();
        var value = new CacheControlHeaderValue();
        options.ConfigureValueForRoutes(value, ["/a", "/b"]);

        options.Routes.Should().HaveCount(2);
        options.Routes["/a"].Should().Be(value);
        options.Routes["/b"].Should().Be(value);
        options.DefaultHeader.Should().BeNull();
    }

    [Fact]
    public void ConfigureNoCacheForRoutes()
    {
        var options = new CacheHeaderOptions();
        options.ConfigureNoCacheForRoutes(["/a", "/b"]);

        var noCache = new CacheControlHeaderValue { NoStore = true, NoCache = true };
        options.Routes.Should().HaveCount(2);
        options.Routes["/a"].Should().Be(noCache);
        options.Routes["/b"].Should().Be(noCache);
        options.DefaultHeader.Should().BeNull();
    }
}