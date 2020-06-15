using System;
using System.Threading.Tasks;
using Fusonic.Extensions.AspNetCore.Http.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Fusonic.Extensions.AspNetCore.Tests.Http
{
    public class CacheHeaderMiddlewareTests
    {
        [Fact]
        public async Task SetDefaultHeaderIfNoSpecificRouteIsSet()
        {
            var defaultHeader = new CacheControlHeaderValue { NoCache = true };
            var httpContext = new DefaultHttpContext();
            var nextCalled = false;

            var middleware = new CacheHeaderMiddleware(new RequestDelegate(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            }), new CacheHeaderOptions()
            {
                DefaultHeader = defaultHeader
            });

            await middleware.Invoke(httpContext);
            Assert.Equal(defaultHeader, httpContext.Response.GetTypedHeaders().CacheControl);
            Assert.True(nextCalled);

            // Does not override exisiting header
            nextCalled = false;
            var newHeader = new CacheControlHeaderValue { NoCache = false, MaxAge = TimeSpan.FromDays(10) };
            await middleware.Invoke(httpContext);
            Assert.Equal(defaultHeader, httpContext.Response.GetTypedHeaders().CacheControl);
            Assert.True(nextCalled);
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
            Assert.Equal(defaultHeader, httpContext.Response.GetTypedHeaders().CacheControl);
            Assert.True(nextCalled);

            nextCalled = false;
            httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/MyFancyRoute/xyz/asdf";
            await middleware.Invoke(httpContext);
            Assert.Equal(routeHeader, httpContext.Response.GetTypedHeaders().CacheControl);
            Assert.True(nextCalled);

            // Check route matching ignores case (Ordinal ignore case)
            httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/myfancyroute/xyz/asdf";
            await middleware.Invoke(httpContext);
            Assert.Equal(routeHeader, httpContext.Response.GetTypedHeaders().CacheControl);
        }

        [Fact]
        public void ConfigureValueForRoutes()
        {
            var options = new CacheHeaderOptions();
            var value = new CacheControlHeaderValue();
            options.ConfigureValueForRoutes(value, new[] { "/a", "/b" });

            Assert.Equal(2, options.Routes.Count);
            Assert.Equal(options.Routes["/a"], value);
            Assert.Equal(options.Routes["/b"], value);
            Assert.Null(options.DefaultHeader);
        }

        [Fact]
        public void ConfigureNoCacheForRoutes()
        {
            var options = new CacheHeaderOptions();
            options.ConfigureNoCacheForRoutes(new[] { "/a", "/b" });

            var noCache = new CacheControlHeaderValue { NoStore = true, NoCache = true };
            Assert.Equal(2, options.Routes.Count);
            Assert.Equal(noCache, options.Routes["/a"]);
            Assert.Equal(noCache, options.Routes["/b"]);
            Assert.Null(options.DefaultHeader);
        }
    }
}