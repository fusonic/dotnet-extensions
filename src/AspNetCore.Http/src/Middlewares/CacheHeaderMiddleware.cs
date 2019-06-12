using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Fusonic.Extensions.AspNetCore.Http.Middlewares
{
    public class CacheHeaderMiddleware
    {
        private readonly RequestDelegate next;
        private readonly CacheHeaderOptions options;

        public CacheHeaderMiddleware(RequestDelegate next, CacheHeaderOptions options)
        {
            this.next = next;
            this.options = options;
        }

        public Task Invoke(HttpContext context)
        {
            if (context.Response.Headers[HeaderNames.CacheControl].Count == 0)
            {
                foreach (var route in options.Routes)
                {
                    if (context.Request.Path.StartsWithSegments(route.Key))
                    {
                        context.Response.GetTypedHeaders().CacheControl = route.Value;
                        return next(context);
                    }
                }

                context.Response.GetTypedHeaders().CacheControl = options.DefaultHeader;
            }

            return next(context);
        }
    }
}