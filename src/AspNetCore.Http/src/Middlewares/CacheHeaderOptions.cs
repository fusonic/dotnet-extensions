﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Fusonic.Extensions.AspNetCore.Http.Middlewares
{
    public class CacheHeaderOptions
    {
        /// <summary> The routes to match. The first route to match will be used. </summary>
        public Dictionary<PathString, CacheControlHeaderValue> Routes { get; } = new Dictionary<PathString, CacheControlHeaderValue>();

        /// <summary> The default header that will be set if no routes match /summary>
        public CacheControlHeaderValue DefaultHeader { get; set; }

        /// <summary>Configures <paramref name="value"/> for the given <paramref name="routes"/></summary>.
        public void ConfigureValueForRoutes(CacheControlHeaderValue value, IEnumerable<string> routes)
        {
            foreach (var route in routes)
            {
                Routes.Add(route, value);
            }
        }

        /// <summary>
        /// Configures <see cref="CacheControlHeaderValue.NoStore"/> = <c>true</c> and <see cref="CacheControlHeaderValue.NoCache"/> = <c>true</c> for the given <paramref name="routes"/>.
        /// </summary>
        public void ConfigureNoCacheForRoutes(IEnumerable<string> routes)
        {
            var noCache = new CacheControlHeaderValue { NoStore = true, NoCache = true };
            foreach (var route in routes)
            {
                Routes.Add(route, noCache);
            }
        }
    }
}