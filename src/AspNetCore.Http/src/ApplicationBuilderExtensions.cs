using System;
using Fusonic.Extensions.AspNetCore.Http.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Fusonic.Extensions.AspNetCore.Http
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCacheHeaders(this IApplicationBuilder app, Action<CacheHeaderOptions>? configure = null)
        {
            var options = new CacheHeaderOptions();
            configure?.Invoke(options);

            return app.UseMiddleware<CacheHeaderMiddleware>(options);
        }
    }
}